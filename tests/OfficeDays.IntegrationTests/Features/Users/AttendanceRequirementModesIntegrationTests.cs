using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class AttendanceRequirementModesIntegrationTests
{
    private const string Path = "/api/users/me/requirements";

    [Fact]
    public async Task Should_PreserveWeeklyChoiceAcrossAccountHistoryAndExports_WhenRegisteringWithWeeklyDays()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        const string expectedMode = "DaysPerWeek";
        const int expectedValue = 3;
        var body = new { username = "weekly", password = "Normal-user-password!", timeZoneId = "UTC", countryCode = "RO", requirementMode = expectedMode, requirementValue = expectedValue };

        // Act
        var registration = await client.PostAsJsonAsync("/api/users", body);
        registration.EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { body.username, body.password });
        login.EnsureSuccessStatusCode();
        var csrf = await Csrf(client);
        var profile = await PutJson(client, "/api/users/me", new { email = "weekly@example.com" }, csrf);
        profile.EnsureSuccessStatusCode();
        var current = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        var history = await client.GetFromJsonAsync<JsonElement>(Path);
        var exportResponse = await client.GetAsync("/api/users/me/export");
        exportResponse.EnsureSuccessStatusCode();
        var export = await Read(exportResponse);

        // Assert
        foreach (var item in new[] { await Read(registration), await Read(login), await Read(profile), current,
                     history.GetProperty("items").EnumerateArray().Single(), export.GetProperty("requirementHistory").EnumerateArray().Single() })
        {
            Assert.Equal(expectedMode, item.GetProperty("requirementMode").GetString());
            Assert.Equal(expectedValue, item.GetProperty("requirementValue").GetInt32());
            Assert.False(item.TryGetProperty("requiredOfficePercentage", out _));
            Assert.False(item.TryGetProperty("mode", out _));
            Assert.False(item.TryGetProperty("value", out _));
        }
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.UserAttendanceRequirements.SingleAsync(x => x.UserId == current.GetProperty("id").GetGuid());
        Assert.Equal(AttendanceRequirementMode.DaysPerWeek, row.RequirementMode);
        Assert.Equal(expectedValue, row.RequirementValue);
    }

    [Fact]
    public async Task Should_ApplyHistoricalModesToStatusAndReports_WhenSwitchingBetweenModes()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "switching");
        await Login(client, "switching", "Normal-user-password!");
        var csrf = await Csrf(client);
        const int expectedWeeklyValue = 3;
        var created = await PostJson(client, Path, new { startingFrom = "2099-09-01", requirementMode = "DaysPerWeek", requirementValue = expectedWeeklyValue }, csrf);
        created.EnsureSuccessStatusCode();
        var next = await PostJson(client, Path, new { startingFrom = "2099-10-01", requirementMode = "Percentage", requirementValue = 40 }, csrf);
        next.EnsureSuccessStatusCode();

        // Act / Assert
        foreach (var (month, percentage, mode, value) in new[] { (9, 60, "DaysPerWeek", 3), (10, 40, "Percentage", 40) })
        {
            var status = await client.GetFromJsonAsync<JsonElement>($"/api/status?year=2099&month={month}");
            var expectedDays = (int)Math.Ceiling(status.GetProperty("eligibleWorkingDays").GetInt32() * percentage / 100m);
            Assert.Equal(expectedDays, status.GetProperty("requiredOfficeDays").GetInt32());
            var reportResponse = await client.GetAsync($"/api/reports?year=2099&startMonth={month}&endMonth={month}");
            reportResponse.EnsureSuccessStatusCode();
            var reportMonth = (await Read(reportResponse)).GetProperty("months").EnumerateArray().Single();
            Assert.False(reportMonth.TryGetProperty("requiredOfficePercentage", out _));
            Assert.Equal(mode, reportMonth.GetProperty("requirementMode").GetString());
            Assert.Equal(value, reportMonth.GetProperty("requirementValue").GetInt32());
            Assert.Equal(expectedDays, reportMonth.GetProperty("requiredOfficeDays").GetInt32());
        }
        var id = (await Read(created)).GetProperty("id").GetGuid();
        var edited = await PutJson(client, $"{Path}/{id}", new { startingFrom = "2099-09-01", requirementMode = "DaysPerWeek", requirementValue = 2 }, csrf);
        edited.EnsureSuccessStatusCode();
        const int expectedEditedValue = 2;
        Assert.Equal(expectedEditedValue, (await Read(edited)).GetProperty("requirementValue").GetInt32());
    }

    [Theory]
    [InlineData("{\"requirementMode\":\"DaysPerWeek\",\"requirementValue\":6}")]
    [InlineData("{\"requirementMode\":\"DaysPerWeek\",\"requirementValue\":-1}")]
    [InlineData("{\"requirementMode\":\"DaysPerWeek\",\"requirementValue\":2.5}")]
    [InlineData("{\"requirementMode\":\"Percentage\",\"requirementValue\":101}")]
    [InlineData("{\"requirementMode\":\"Unknown\",\"requirementValue\":3}")]
    [InlineData("{\"requirementMode\":\"DaysPerWeek\"}")]
    [InlineData("{\"requirementValue\":3}")]
    [InlineData("{\"requiredOfficePercentage\":60}")]
    [InlineData("{}")]
    public async Task Should_RejectInvalidRequirementsWithoutChanges_WhenPayloadIsInvalid(string json)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "invalid-mode");
        await Login(client, "invalid-mode", "Normal-user-password!");
        var csrf = await Csrf(client);
        var initial = await client.GetStringAsync(Path);
        var baselineId = JsonDocument.Parse(initial).RootElement.GetProperty("items")[0].GetProperty("id").GetGuid();
        var body = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? throw new InvalidOperationException("Invalid test payload.");
        body["startingFrom"] = "2099-10-01";
        const HttpStatusCode expectedStatus = HttpStatusCode.BadRequest;

        // Act
        var create = await PostJson(client, Path, body, csrf);
        body["startingFrom"] = "0001-01-01";
        var update = await PutJson(client, $"{Path}/{baselineId}", body, csrf);

        // Assert
        Assert.Equal(expectedStatus, create.StatusCode);
        Assert.Equal(expectedStatus, update.StatusCode);
        Assert.Equal(initial, await client.GetStringAsync(Path));
    }

    [Fact]
    public async Task Should_PreserveAllHistoryAndConstraints_WhenMigratingExistingPercentages()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261016072600_AddOfficeRequirementHistory");
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var expectedDate = new DateOnly(2026, 11, 1);
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero);
        const int expectedValue = 37;
        const string preferences = "{\"TeamDay\":null}";
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO Users (Id, Username, NormalizedUsername, PasswordHash, TimeZoneId, HolidayJurisdictionId, IsAdmin, CreatedAt, Preferences)
            VALUES ({userId}, 'migration', 'MIGRATION', 'hash', 'UTC', 1, 0, {expectedCreatedAt}, {preferences});
            """);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO UserRequiredOfficePercentages (Id, UserId, StartingFrom, RequiredOfficePercentage, CreatedAt)
            VALUES ({id}, {userId}, {expectedDate}, {expectedValue}, {expectedCreatedAt});
            """);

        // Act
        await migrator.MigrateAsync();
        var row = await db.UserAttendanceRequirements.AsNoTracking().SingleAsync();

        // Assert
        Assert.Equal(id, row.Id);
        Assert.Equal(userId, row.UserId);
        Assert.Equal(expectedDate, row.StartingFrom);
        Assert.Equal(expectedCreatedAt, row.CreatedAt);
        Assert.Equal(expectedValue, row.RequirementValue);
        Assert.Equal(AttendanceRequirementMode.Percentage, row.RequirementMode);
        Assert.False(db.Database.HasPendingModelChanges());
        await Assert.ThrowsAsync<SqliteException>(() => db.Database.ExecuteSqlRawAsync("UPDATE UserAttendanceRequirements SET RequirementMode = 'DaysPerWeek', RequirementValue = 6"));
        await Assert.ThrowsAsync<SqliteException>(() => db.Database.ExecuteSqlRawAsync("UPDATE UserAttendanceRequirements SET RequirementMode = 'Unknown'"));
        await db.Database.ExecuteSqlRawAsync("UPDATE UserAttendanceRequirements SET RequirementMode = 'DaysPerWeek', RequirementValue = 3");
        // Renaming the columns in either direction must preserve the configured weekly value.
        await migrator.MigrateAsync("20261021173735_AddAttendanceRequirementModes");
        await migrator.MigrateAsync();
        var renamed = await db.UserAttendanceRequirements.AsNoTracking().SingleAsync();
        const int expectedWeeklyValue = 3;
        Assert.Equal(id, renamed.Id);
        Assert.Equal(expectedWeeklyValue, renamed.RequirementValue);
        Assert.Equal(AttendanceRequirementMode.DaysPerWeek, renamed.RequirementMode);
        await migrator.MigrateAsync("20261016072600_AddOfficeRequirementHistory");
        await migrator.MigrateAsync();
        var restored = await db.UserAttendanceRequirements.AsNoTracking().SingleAsync();
        const int expectedDowngradedPercentage = 60;
        Assert.Equal(id, restored.Id);
        Assert.Equal(expectedDowngradedPercentage, restored.RequirementValue);
        Assert.Equal(AttendanceRequirementMode.Percentage, restored.RequirementMode);
    }
}
