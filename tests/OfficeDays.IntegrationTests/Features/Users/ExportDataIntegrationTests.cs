using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Reports.GetReport;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Features.Users.ExportData.Contracts;
using OfficeDays.Services;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class ExportDataIntegrationTests
{
    [Fact]
    public async Task Should_UseSameVersionService_WhenExportingDataAndReadingVersion()
    {
        // Arrange
        const string expectedVersion = "9.8.7+test-build";
        await using var original = new OfficeDaysFactory();
        await using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                                                                                                       services.AddSingleton<IAppVersionService>(new TestAppVersionService())));
        using var client = factory.CreateClient();
        await CreateUser(client, "versionedexport");
        await Login(client, "versionedexport", "Normal-user-password!");

        // Act
        var version = await client.GetFromJsonAsync<GetVersionResponse>("/api/version");
        var export = await client.GetFromJsonAsync<PersonalDataExport>("/api/users/me/export");

        // Assert
        Assert.NotNull(version);
        Assert.NotNull(export);
        Assert.Equal(expectedVersion, version.Version);
        Assert.Equal(expectedVersion, export.Application.Version);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_RejectExport_WhenSessionIsAbsentOrAccountDeleted(bool accountDeleted)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        const HttpStatusCode expectedStatusCode = HttpStatusCode.Unauthorized;
        if (accountDeleted)
        {
            await CreateUser(client, "deleted");
            await Login(client, "deleted", "Normal-user-password!");
            await DeleteUser(factory, "deleted");
        }

        // Act
        var response = await client.GetAsync("/api/users/me/export");

        // Assert: deleted-account cookies are rejected before the export handler runs.
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Null(response.Content.Headers.ContentDisposition);
    }

    [Fact]
    public async Task Should_ExportNullsAndEmptyCollections_WhenAccountHasNoPersonalRecords()
    {
        // Arrange
        var expectedBaselineDate = DateOnly.MinValue;
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "empty");
        await Login(client, "empty", "Normal-user-password!");

        // Act
        var export = await client.GetFromJsonAsync<PersonalDataExport>("/api/users/me/export");

        // Assert
        Assert.NotNull(export);
        Assert.Empty(export.Attendance);
        Assert.Empty(export.Plans);
        Assert.Empty(export.Vacations);
        Assert.Empty(export.HolidayCalendar.Holidays);
        Assert.Null(export.Preferences.TeamDay);
        Assert.Equal(expectedBaselineDate, Assert.Single(export.RequirementHistory).StartingFrom);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_ExportOnlyOwnedDataAndRecreateReport_WhenAccountHasPersonalRecords(bool isAdmin)
    {
        // Arrange
        const string expectedMediaType = "application/json";
        const string expectedDispositionType = "attachment";
        const string expectedFileExtension = ".json";
        const string expectedFormat = "officedays-personal-data";
        const int expectedFormatVersion = 1;
        const string expectedTeamDay = "thursday";
        const int expectedAttendanceCount = 3;
        const int expectedPlanCount = 2;
        const int expectedRequirementCount = 2;
        const int expectedHolidayCount = 3;
        var expectedExportedAt = new DateTimeOffset(2099, 9, 15, 9, 0, 0, TimeSpan.Zero);
        var expectedRegistrationDate = new DateOnly(2099, 1, 1);
        var expectedLocalDate = new DateOnly(2099, 9, 15);
        var expectedVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                              ?? throw new InvalidOperationException("The application assembly must declare its version.");
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "exporter");
        await CreateUser(client, "other");
        Guid expectedOwnerId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = await db.Users
                                .Where(user => user.Username == "exporter" || user.Username == "other")
                                .ToListAsync();
            var owner = users.Single(x => x.Username == "exporter");
            expectedOwnerId = owner.Id;
            db.Entry(owner).Property(x => x.IsAdmin).CurrentValue = isAdmin;
            foreach (var user in users)
            {
                db.Entry(user).Property(x => x.CreatedAt).CurrentValue = new DateTimeOffset(2098, 12, 31, 23, 30, 0, TimeSpan.Zero);
                user.Preferences.TeamDay = DayOfWeek.Thursday;
                db.Attendances.AddRange(new[] { new DateOnly(2099, 9, 1), new DateOnly(2099, 9, 2), new DateOnly(2100, 1, 1) }
                    .Select(date => new OfficeDays.Domain.Attendance
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Date = date,
                        IsManual = true,
                        CreatedAt = user.CreatedAt
                    }));
                db.OfficePlans.AddRange(new[] { new DateOnly(2098, 1, 1), new DateOnly(2100, 1, 1) }
                    .Select(date => new OfficePlan
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Date = date,
                        CreatedAt = user.CreatedAt
                    }));
                db.Vacations.Add(new Vacation
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    From = new DateOnly(2099, 8, 31),
                    To = new DateOnly(2099, 9, 2),
                    CreatedAt = user.CreatedAt
                });
                db.UserAttendanceRequirements.Add(new UserAttendanceRequirement
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    StartingFrom = new DateOnly(2099, 10, 1),
                    RequirementMode = AttendanceRequirementMode.DaysPerWeek,
                    RequirementValue = 3,
                    CreatedAt = user.CreatedAt
                });
                db.ApiTokens.Add(new ApiToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Name = "secret-token-name",
                    TokenHash = $"secret-token-hash-{user.Id}",
                    CreatedAt = user.CreatedAt
                });
            }
            var jurisdiction = users[0].HolidayJurisdictionId;
            db.BankHolidays.AddRange(new[] { new DateOnly(2098, 1, 1), new DateOnly(2099, 9, 3), new DateOnly(2100, 1, 1) }
                .Select(date => new BankHoliday
                {
                    Id = Guid.NewGuid(),
                    HolidayJurisdictionId = jurisdiction,
                    Date = date,
                    Name = "Shared holiday"
                }));
            var otherCalendar = new HolidayJurisdiction
            {
                Code = "XX",
                Name = "Unrelated calendar"
            };
            db.HolidayJurisdictions.Add(otherCalendar);
            await db.SaveChangesAsync();
            db.BankHolidays.Add(new BankHoliday
            {
                Id = Guid.NewGuid(),
                HolidayJurisdictionId = otherCalendar.Id,
                Date = new DateOnly(2099, 1, 1),
                Name = "Unrelated holiday"
            });
            await db.SaveChangesAsync();
        }
        await Login(client, "exporter", "Normal-user-password!");
        var originalReport = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&startMonth=1&endMonth=12");
        var expectedReportJson = originalReport.GetRawText();
        using var services = factory.Services.CreateScope();
        var calculator = services.ServiceProvider.GetRequiredService<IReportCalculator>();

        // Act
        var response = await client.GetAsync("/api/users/me/export?userId=ignored");
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        var export = JsonSerializer.Deserialize<PersonalDataExport>(raw, jsonOptions)
                     ?? throw new InvalidOperationException("The export response must contain personal data.");

        // Reconstruct exclusively from the downloaded document, without querying the source database.
        var reconstructedUser = new User
        {
            Id = export.Profile.Id,
            Username = export.Profile.Username,
            NormalizedUsername = export.Profile.NormalizedUsername,
            PasswordHash = "unused",
            TimeZoneId = export.Profile.TimeZoneId,
            CreatedAt = export.Profile.CreatedAt
        };
        var report = calculator.Calculate(
            2099, 1, 12, reconstructedUser, export.ExportedAt,
            export.RequirementHistory.Select(requirement => new UserAttendanceRequirement
            {
                Id = requirement.Id,
                UserId = requirement.UserId,
                StartingFrom = requirement.StartingFrom,
                RequirementMode = requirement.RequirementMode,
                RequirementValue = requirement.RequirementValue,
                CreatedAt = requirement.CreatedAt
            }),
            export.HolidayCalendar.Holidays.Select(holiday => holiday.Date),
            export.Vacations.Select(vacation => (vacation.From, vacation.To)),
            export.Attendance.Select(attendance => attendance.Date));
        var reconstructedReportJson = JsonSerializer.Serialize(report.ToViewModel(), jsonOptions);

        // Assert
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Equal(expectedMediaType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedDispositionType, response.Content.Headers.ContentDisposition?.DispositionType);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar;
        Assert.NotNull(fileName);
        Assert.Contains(expectedFileExtension, fileName);
        foreach (var secret in new[] { "password", "token", "secret-token", "other@example.com", "Unrelated" })
        {
            Assert.DoesNotContain(secret, raw, StringComparison.OrdinalIgnoreCase);
        }
        Assert.Equal(expectedFormat, export.Format);
        Assert.Equal(expectedFormatVersion, export.FormatVersion);
        Assert.Equal(expectedVersion, export.Application.Version);
        Assert.Equal(expectedExportedAt, export.ExportedAt);
        Assert.Equal(expectedRegistrationDate, export.ReportContext.RegistrationLocalDate);
        Assert.Equal(expectedLocalDate, export.ReportContext.LocalDate);
        Assert.Equal(expectedOwnerId, export.Profile.Id);
        Assert.Equal(isAdmin, export.Profile.IsAdmin);
        Assert.Equal(expectedTeamDay, export.Preferences.TeamDay);
        Assert.Equal(expectedAttendanceCount, export.Attendance.Length);
        Assert.Equal(expectedPlanCount, export.Plans.Length);
        Assert.Single(export.Vacations);
        Assert.Equal(expectedRequirementCount, export.RequirementHistory.Length);
        Assert.Equal(expectedHolidayCount, export.HolidayCalendar.Holidays.Length);
        Assert.All(export.Attendance, attendance => Assert.Equal(expectedOwnerId, attendance.UserId));
        Assert.All(export.Plans, plan => Assert.Equal(expectedOwnerId, plan.UserId));
        Assert.All(export.Vacations, vacation => Assert.Equal(expectedOwnerId, vacation.UserId));
        Assert.All(export.RequirementHistory, requirement => Assert.Equal(expectedOwnerId, requirement.UserId));
        var expectedJurisdictionId = export.Profile.HolidayJurisdictionId;
        Assert.All(export.HolidayCalendar.Holidays, holiday => Assert.Equal(expectedJurisdictionId, holiday.HolidayJurisdictionId));
        Assert.Equal(expectedReportJson, reconstructedReportJson);
    }

    private sealed class TestAppVersionService : IAppVersionService
    {
        public string Version => "9.8.7+test-build";
    }
}
