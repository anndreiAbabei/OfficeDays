using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Status;

public sealed class StatusIntegrationTests
{
    [Fact]
    public async Task Should_CreditWeekendVisitsOnceAndKeepDashboardAndReportsConsistent_WhenRecordingAttendance()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "weekend-owner");
        await Login(client, "weekend-owner", "Normal-user-password!");
        var csrf = await Csrf(client);
        const string statusPath = "/api/status?year=2099&month=9";
        var before = await client.GetFromJsonAsync<JsonElement>(statusPath, TestContext.Current.CancellationToken);
        const int expectedOffice = 3;
        const int expectedStored = 3;
        const int expectedRemaining = 8;
        const decimal expectedProgress = 27.3m;
        const decimal expectedAttendancePercentage = 13.6m;
        const bool expectedAttended = true;
        string[] dates = ["2099-09-04", "2099-09-05", "2099-09-06"];

        // Act: Friday, Saturday and Sunday, each recorded twice.
        foreach (var date in dates)
        {
            (await PutJson(client, $"/api/attendance/{date}", new { }, csrf)).EnsureSuccessStatusCode();
            (await PutJson(client, $"/api/attendance/{date}", new { }, csrf)).EnsureSuccessStatusCode();
        }
        var status = await client.GetFromJsonAsync<JsonElement>(statusPath, TestContext.Current.CancellationToken);
        var report = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&startMonth=9&endMonth=9", TestContext.Current.CancellationToken);
        var stored = await client.GetFromJsonAsync<JsonElement>("/api/attendance?year=2099&month=9", TestContext.Current.CancellationToken);
        var calendar = await client.GetFromJsonAsync<JsonElement>("/api/planning?year=2099&month=9", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStored, stored.GetArrayLength());
        Assert.Equal(expectedOffice, status.GetProperty("officeDays").GetInt32());
        Assert.Equal(expectedRemaining, status.GetProperty("remainingOfficeDays").GetInt32());
        Assert.Equal(expectedProgress, status.GetProperty("progressPercentage").GetDecimal());
        foreach (var property in new[] { "eligibleWorkingDays", "requiredOfficeDays", "maximumWfhDays" })
            Assert.Equal(before.GetProperty(property).GetInt32(), status.GetProperty(property).GetInt32());
        var month = Assert.Single(report.GetProperty("months").EnumerateArray());
        Assert.Equal(expectedOffice, month.GetProperty("officeDays").GetInt32());
        Assert.Equal(expectedOffice, report.GetProperty("summary").GetProperty("officeDays").GetInt32());
        Assert.Equal(expectedAttendancePercentage, month.GetProperty("attendancePercentage").GetDecimal());
        foreach (var property in new[] { "eligibleWorkingDays", "requiredOfficeDays" })
            Assert.Equal(status.GetProperty(property).GetInt32(), month.GetProperty(property).GetInt32());
        foreach (var date in dates)
            Assert.Equal(expectedAttended, calendar.GetProperty("days").EnumerateArray()
                .Single(day => day.GetProperty("date").GetString() == date).GetProperty("isAttended").GetBoolean());

        using var other = factory.CreateClient();
        await CreateUser(other, "weekend-other");
        await Login(other, "weekend-other", "Normal-user-password!");
        var otherStatus = await other.GetFromJsonAsync<JsonElement>(statusPath, TestContext.Current.CancellationToken);
        const int expectedOtherOffice = 0;
        Assert.Equal(expectedOtherOffice, otherStatus.GetProperty("officeDays").GetInt32());
    }

    [Fact]
    public async Task Status_uses_persisted_holidays_vacations_and_attendance()
    {
        await using var factory = new OfficeDaysFactory();
        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        await PutJson(admin, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-09-14", name = "Holiday" } }, adminCsrf);
        await PostJson(admin, "/api/holiday-jurisdictions",
            new { code = "GB-NIR", name = "Northern Ireland" }, adminCsrf);
        await PutJson(admin, "/api/bank-holidays/GB-NIR/2026",
            new[] { new { date = "2026-09-15", name = "Other jurisdiction holiday" } }, adminCsrf);

        using var user = factory.CreateClient();
        await CreateUser(user, "statususer"); await Login(user, "statususer", "Normal-user-password!");
        var csrf = await Csrf(user);
        await PostJson(user, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf);
        await PutJson(user, "/api/attendance/2026-09-01", new { }, csrf);
        await PutJson(user, "/api/attendance/2026-09-02", new { }, csrf);
        await PutJson(user, "/api/attendance/2026-09-14", new { }, csrf); // holiday, does not count

        var status = await user.GetFromJsonAsync<JsonElement>("/api/status?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(16, status.GetProperty("eligibleWorkingDays").GetInt32()); // 22 weekdays - 1 holiday - 5 vacation
        Assert.Equal(8, status.GetProperty("maximumWfhDays").GetInt32());
        Assert.Equal(8, status.GetProperty("requiredOfficeDays").GetInt32());
        Assert.Equal(2, status.GetProperty("officeDays").GetInt32());
        Assert.Equal(6, status.GetProperty("remainingOfficeDays").GetInt32());
    }

    [Fact]
    public async Task Should_UseCurrentMonthAndRejectInvalidRequests_WhenRequestingStatus()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "statusdefaults");
        await Login(client, "statusdefaults", "Normal-user-password!");

        var current = await client.GetAsync("/api/status", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal("2099-09", (await Read(current)).GetProperty("period").GetString());

        var incomplete = await client.GetAsync("/api/status?year=2099", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        foreach (var month in new[] { 0, 13, 9998 })
        {
            var invalid = await client.GetAsync($"/api/status?year=2099&month={month}", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            Assert.True((await Read(invalid)).GetProperty("errors").TryGetProperty("Month", out _));
        }

        var expectedUnauthorized = HttpStatusCode.Unauthorized;
        await DeleteUser(factory, "statusdefaults");
        Assert.Equal(expectedUnauthorized, (await client.GetAsync("/api/status", TestContext.Current.CancellationToken)).StatusCode);
    }

}
