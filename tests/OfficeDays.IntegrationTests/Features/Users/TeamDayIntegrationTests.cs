using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class TeamDayIntegrationTests
{
    [Fact]
    public async Task Team_day_defaults_saves_persists_and_clears_without_affecting_other_users()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "teamuser");
        await CreateUser(client, "otheruser");
        await Login(client, "teamuser", "Normal-user-password!");
        var csrf = await Csrf(client);
        Assert.Equal(JsonValueKind.Null, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("teamDay").ValueKind);

        foreach (var day in new[] { "monday", "tuesday", "wednesday", "thursday", "friday" })
        {
            var response = await PutJson(client, "/api/users/me", new { teamDay = day }, csrf);
            response.EnsureSuccessStatusCode();
            Assert.Equal(day, (await Read(response)).GetProperty("teamDay").GetString());
            Assert.Equal(day, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("teamDay").GetString());
        }

        var omitted = await PutJson(client, "/api/users/me", new { email = "updated@example.com" }, csrf);
        Assert.Equal("friday", (await Read(omitted)).GetProperty("teamDay").GetString());
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "teamuser", password = "Normal-user-password!" });
        Assert.Equal("friday", (await Read(login)).GetProperty("teamDay").GetString());

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(DayOfWeek.Friday, (await db.Users.SingleAsync(x => x.Username == "teamuser")).Preferences.TeamDay);
            Assert.Null((await db.Users.SingleAsync(x => x.Username == "otheruser")).Preferences.TeamDay);
            Assert.Empty(await db.OfficePlans.ToListAsync());
        }

        csrf = await Csrf(client);
        var cleared = await PutJson(client, "/api/users/me", new { teamDay = (string?)null }, csrf);
        Assert.Equal(JsonValueKind.Null, (await Read(cleared)).GetProperty("teamDay").ValueKind);
        Assert.Equal(JsonValueKind.Null, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("teamDay").ValueKind);
    }

    [Fact]
    public async Task Migration_initializes_preferences_for_existing_users()
    {
        await using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        await migrator.MigrateAsync("20261010191519_AddOfficePlans");
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO Users (Id, Username, NormalizedUsername, PasswordHash, TimeZoneId, HolidayJurisdictionId, IsAdmin, CreatedAt, RequiredOfficePercentage)
            VALUES ('00000000-0000-0000-0000-000000000001', 'existing', 'EXISTING', 'hash', 'UTC', 1, 0, '2026-10-01 00:00:00+00:00', 50);
            """);
        await migrator.MigrateAsync();
        var user = await db.Users.SingleAsync();
        Assert.NotNull(user.Preferences);
        Assert.Null(user.Preferences.TeamDay);
        user.Preferences.TeamDay = DayOfWeek.Thursday;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(DayOfWeek.Thursday, (await db.Users.SingleAsync()).Preferences.TeamDay);
    }

    [Theory]
    [InlineData("saturday")]
    [InlineData("sunday")]
    [InlineData("invalid")]
    [InlineData("")]
    public async Task Invalid_team_day_does_not_overwrite_saved_preference(string day)
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "teamuser");
        await Login(client, "teamuser", "Normal-user-password!");
        var csrf = await Csrf(client);
        (await PutJson(client, "/api/users/me", new { teamDay = "thursday" }, csrf)).EnsureSuccessStatusCode();
        var invalid = await PutJson(client, "/api/users/me", new { teamDay = day }, csrf);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("thursday", (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("teamDay").GetString());
    }
}
