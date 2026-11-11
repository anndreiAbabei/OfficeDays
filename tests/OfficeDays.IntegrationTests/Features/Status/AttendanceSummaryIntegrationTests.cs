using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OfficeDays.Domain;
using OfficeDays.Features.Status.GetStatus.Contracts;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Status;

public sealed class AttendanceSummaryIntegrationTests
{
    [Fact]
    public async Task Should_ExplainPersistedCalendarAndIsolateUsers_WhenReadingSummaryWithCookieOrBearer()
    {
        // Arrange
        await using var root = new OfficeDaysFactory();
        await using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2099, 10, 30, 9, 0, 0, TimeSpan.Zero)));
        }));
        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        (await PutJson(admin, "/api/bank-holidays/RO/2099", new[]
        {
            new { date = "2099-10-01", name = "Weekday holiday" },
            new { date = "2099-10-03", name = "Weekend holiday" }
        }, adminCsrf)).EnsureSuccessStatusCode();
        (await PostJson(admin, "/api/holiday-jurisdictions", new { code = "GB-NIR", name = "Northern Ireland" }, adminCsrf)).EnsureSuccessStatusCode();
        (await PutJson(admin, "/api/bank-holidays/GB-NIR/2099", new[] { new { date = "2099-10-30", name = "Other calendar" } }, adminCsrf)).EnsureSuccessStatusCode();

        using var owner = factory.CreateClient();
        await CreateUser(owner, "summary-owner");
        await Login(owner, "summary-owner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        (await PostJson(owner, "/api/vacations", new { from = "2099-09-30", to = "2099-10-05" }, csrf)).EnsureSuccessStatusCode();
        foreach (var date in new[] { "2099-10-01", "2099-10-06", "2099-10-07" })
            (await PutJson(owner, $"/api/attendance/{date}", new { }, csrf)).EnsureSuccessStatusCode();
        (await PutJson(owner, "/api/planning/2099-10-30", new { }, csrf)).EnsureSuccessStatusCode();
        var tokenResponse = await PostJson(owner, "/api/tokens", new { name = "Summary reader" }, csrf);
        tokenResponse.EnsureSuccessStatusCode();
        var token = (await Read(tokenResponse)).GetProperty("token").GetString();

        using var other = factory.CreateClient();
        await CreateUser(other, "summary-other");
        await Login(other, "summary-other", "Normal-user-password!");
        (await PostJson(other, "/api/vacations", new { from = "2099-10-29", to = "2099-10-31" }, await Csrf(other))).EnsureSuccessStatusCode();
        using var bearer = factory.CreateClient();
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        const string path = "/api/status?year=2099&month=10";

        // Act
        var response = await owner.GetAsync(path, TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<GetStatusResponse>(TestContext.Current.CancellationToken);
        var bearerResult = await bearer.GetStringAsync(path, TestContext.Current.CancellationToken);
        var otherResult = await other.GetFromJsonAsync<GetStatusResponse>(path, TestContext.Current.CancellationToken);

        // Assert
        const HttpStatusCode expectedStatus = HttpStatusCode.OK;
        const int expectedEligible = 19;
        const int expectedRequired = 10;
        const int expectedOffice = 2;
        const int expectedRemaining = 8;
        const int expectedPlanned = 1;
        const int expectedTotalDays = 31;
        const int expectedWeekdays = 22;
        const int expectedHolidayWeekdays = 1;
        const int expectedVacationWeekdays = 2;
        const int expectedBaseline = 11;
        const int expectedAvailable = 1;
        const int expectedOutstanding = 7;
        const int expectedPercentage = 50;
        const int expectedOtherCount = 0;
        const AttendanceRequirementMode expectedMode = AttendanceRequirementMode.Percentage;
        var expectedToday = new DateOnly(2099, 10, 30);
        GetStatusHoliday[] expectedHolidays = [new GetStatusHoliday(new DateOnly(2099, 10, 1), "Weekday holiday"),
                                              new GetStatusHoliday(new DateOnly(2099, 10, 3), "Weekend holiday")];
        var expectedVacationDates = Enumerable.Range(1, 5).Select(day => new DateOnly(2099, 10, day)).ToArray();
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(expectedEligible, result.EligibleWorkingDays);
        Assert.Equal(expectedRequired, result.RequiredOfficeDays);
        Assert.Equal(expectedOffice, result.OfficeDays);
        Assert.Equal(expectedRemaining, result.RemainingOfficeDays);
        Assert.Equal(expectedPlanned, result.UpcomingPlannedDays);
        Assert.Equal(expectedToday, result.Summary.Today);
        Assert.Equal(expectedTotalDays, result.Summary.TotalDays);
        Assert.Equal(expectedWeekdays, result.Summary.WorkingDays);
        Assert.Equal(expectedHolidayWeekdays, result.Summary.BankHolidayWorkingDays);
        Assert.Equal(expectedVacationWeekdays, result.Summary.VacationWorkingDays);
        Assert.Equal(expectedMode, result.Summary.RequirementMode);
        Assert.Equal(expectedPercentage, result.Summary.RequirementValue);
        Assert.Equal(expectedBaseline, result.Summary.RequiredOfficeDaysWithoutAbsences);
        Assert.Equal(expectedPlanned, result.Summary.PlannedDays);
        Assert.Equal(expectedAvailable, result.Summary.RemainingEligibleDays);
        Assert.Equal(expectedOutstanding, result.Summary.OutstandingOfficeDays);
        Assert.Equal(expectedHolidays, result.Summary.BankHolidays);
        Assert.Equal(expectedVacationDates, result.Summary.VacationDates);
        Assert.Equal(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), bearerResult);
        Assert.NotNull(otherResult);
        Assert.Equal(expectedOtherCount, otherResult.OfficeDays);
        Assert.Equal(expectedOtherCount, otherResult.Summary.PlannedDays);
        Assert.DoesNotContain(expectedVacationDates[0], otherResult.Summary.VacationDates);
        Assert.DoesNotContain(new DateOnly(2099, 10, 30), result.Summary.VacationDates);
    }

    [Theory]
    [InlineData("Pacific/Auckland", "2100-01-01", "2100-01")]
    [InlineData("America/Los_Angeles", "2099-12-31", "2099-12")]
    public async Task Should_UseUserTodayAtYearBoundary_WhenDefaultingSummaryMonth(string timeZone, string expectedToday, string expectedPeriod)
    {
        // Arrange
        await using var root = new OfficeDaysFactory();
        await using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2099, 12, 31, 12, 0, 0, TimeSpan.Zero)));
        }));
        using var client = factory.CreateClient();
        const string username = "summary-zone";
        var registration = await client.PostAsJsonAsync("/api/users", new
        {
            username, password = "Normal-user-password!", timeZoneId = timeZone, countryCode = "RO"
        }, cancellationToken: TestContext.Current.CancellationToken);
        registration.EnsureSuccessStatusCode();
        await Login(client, username, "Normal-user-password!");

        // Act
        var response = await client.GetFromJsonAsync<GetStatusResponse>("/api/status", TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedPeriod, response.Period);
        Assert.Equal(DateOnly.Parse(expectedToday), response.Summary.Today);
    }

    [Fact]
    public async Task Should_RejectAnonymousSummaryReads_WhenNoAuthenticationIsProvided()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        const HttpStatusCode expectedStatus = HttpStatusCode.Unauthorized;

        // Act
        var response = await client.GetAsync("/api/status", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }
}
