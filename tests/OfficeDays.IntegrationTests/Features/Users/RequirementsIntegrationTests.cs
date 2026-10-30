using System.Diagnostics.CodeAnalysis;
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
using OfficeDays.Services;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "It's a test")]
public sealed class RequirementsIntegrationTests
{
    private const string Path = "/api/users/me/requirements";
    private static async Task<JsonElement[]> Items(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>(Path)).GetProperty("items").EnumerateArray().ToArray();
    private static object Body(string date, int percentage) => new { startingFrom = date, requirementMode = "Percentage", requirementValue = percentage };
    private static async Task<string> SignIn(HttpClient client, string name = "history")
    {
        await CreateUser(client, name);
        await Login(client, name, "Normal-user-password!");
        return await Csrf(client);
    }
    private static async Task AssertTarget(HttpClient client, int year, int month, int percentage)
    {
        var status = await client.GetFromJsonAsync<JsonElement>($"/api/status?year={year}&month={month}");
        Assert.Equal((int)Math.Ceiling(status.GetProperty("eligibleWorkingDays").GetInt32() * percentage / 100m),
            status.GetProperty("requiredOfficeDays").GetInt32());
    }

    [Fact]
    public async Task Schedule_crud_resolves_past_current_future_and_baseline_without_registration_cutoff()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var csrf = await SignIn(client);
        var baseline = Assert.Single(await Items(client));
        Assert.True(baseline.GetProperty("isBaseline").GetBoolean());
        Assert.Equal("0001-01-01", baseline.GetProperty("startingFrom").GetString());
        var baselineId = baseline.GetProperty("id").GetGuid();
        (await PutJson(client, $"{Path}/{baselineId}", Body("0001-01-01", 40), csrf)).EnsureSuccessStatusCode();
        var earlier = await PostJson(client, Path, Body("2099-01-01", 60), csrf);
        Assert.Equal(HttpStatusCode.Created, earlier.StatusCode);
        var earlierId = (await Read(earlier)).GetProperty("id").GetGuid();
        var future = await PostJson(client, Path, Body("2100-01-01", 80), csrf);
        Assert.Equal(HttpStatusCode.Created, future.StatusCode);
        var futureId = (await Read(future)).GetProperty("id").GetGuid();
        Assert.Equal(60, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me", cancellationToken: TestContext.Current.CancellationToken)).GetProperty("requirementValue").GetInt32());
        await AssertTarget(client, 2020, 1, 40);
        await AssertTarget(client, 2099, 1, 60);
        await AssertTarget(client, 2099, 9, 60);
        await AssertTarget(client, 2100, 1, 80);
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "history", password = "Normal-user-password!" }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(60, (await Read(login)).GetProperty("requirementValue").GetInt32());
        csrf = await Csrf(client);
        var profile = await PutJson(client, "/api/users/me", new { email = "new@example.com" }, csrf);
        Assert.Equal(60, (await Read(profile)).GetProperty("requirementValue").GetInt32());
        var edited = await PutJson(client, $"{Path}/{futureId}", Body("2099-09-01", 75), csrf);
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        await AssertTarget(client, 2099, 8, 60);
        await AssertTarget(client, 2099, 9, 75);
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(client, $"{Path}/{earlierId}", csrf)).StatusCode);
        await AssertTarget(client, 2099, 8, 40);
        await AssertTarget(client, 2099, 9, 75);
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(client, $"{Path}/{futureId}", csrf)).StatusCode);
        Assert.Single(await Items(client));
        await AssertTarget(client, 2100, 1, 40);
    }

    [Fact]
    public async Task Duplicate_months_and_baseline_mutations_are_rejected_without_changing_data()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var csrf = await SignIn(client);
        var baselineId = (await Items(client))[0].GetProperty("id").GetGuid();
        var first = await PostJson(client, Path, Body("2099-01-01", 60), csrf);
        var id = (await Read(first)).GetProperty("id").GetGuid();
        (await PostJson(client, Path, Body("2099-02-01", 70), csrf)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await PostJson(client, Path, Body("2099-01-01", 80), csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await PutJson(client, $"{Path}/{id}", Body("2099-02-01", 80), csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Delete(client, $"{Path}/{baselineId}", csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, $"{Path}/{baselineId}", Body("2098-01-01", 80), csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, $"{Path}/{id}", Body("0001-01-01", 80), csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostJson(client, Path, Body("0001-01-01", 80), csrf)).StatusCode);
        // Legacy percentage fields are ignored by the profile contract and must not alter history.
        Assert.Equal(HttpStatusCode.OK, (await PutJson(client, "/api/users/me", new { requirementMode = "Percentage", requirementValue = 10 }, csrf)).StatusCode);
        Assert.Equal(3, (await Items(client)).Length);
        await AssertTarget(client, 2099, 1, 60);
        await AssertTarget(client, 2098, 1, 50);
    }

    [Theory]
    [InlineData("2099-01-15", 50)]
    [InlineData("2099-01-01", -1)]
    [InlineData("2099-01-01", 101)]
    public async Task Invalid_input_is_rejected_for_create_and_edit(string date, int percentage)
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var csrf = await SignIn(client);
        var id = (await Items(client))[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await PostJson(client, Path, Body(date, percentage), csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, $"{Path}/{id}", Body(date, percentage), csrf)).StatusCode);
        Assert.Single(await Items(client));
    }

    [Fact]
    public async Task All_endpoints_require_authentication_mutations_require_csrf_and_rows_are_private()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Path, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(Path, Body("2099-01-01", 50), cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsJsonAsync($"{Path}/{Guid.NewGuid()}", Body("2099-01-01", 50), cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.DeleteAsync($"{Path}/{Guid.NewGuid()}", TestContext.Current.CancellationToken)).StatusCode);
        var csrf = await SignIn(client);
        var baselineId = (await Items(client))[0].GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Path, Body("2099-01-01", 50), cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"{Path}/{baselineId}", Body("0001-01-01", 50), cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync($"{Path}/{baselineId}", TestContext.Current.CancellationToken)).StatusCode);
        var created = await PostJson(client, Path, Body("2099-01-01", 60), csrf);
        var id = (await Read(created)).GetProperty("id").GetGuid();
        using var other = factory.CreateClient();
        var otherCsrf = await SignIn(other, "other");
        Assert.Single(await Items(other));
        foreach (var foreignId in new[] { id, baselineId, Guid.NewGuid() })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await PutJson(other, $"{Path}/{foreignId}", Body("2099-02-01", 10), otherCsrf)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await Delete(other, $"{Path}/{foreignId}", otherCsrf)).StatusCode);
        }
        Assert.Equal(2, (await Items(client)).Length);
        await AssertTarget(client, 2099, 1, 60);
    }

    [Fact]
    public async Task Migration_preserves_all_percentages_and_backfills_one_baseline_per_user()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261015154740_AddUserPreferences", TestContext.Current.CancellationToken);
        foreach (var percentage in new[] { 0, 37, 100 })
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO Users (Id, Username, NormalizedUsername, PasswordHash, TimeZoneId, HolidayJurisdictionId, IsAdmin, CreatedAt, RequiredOfficePercentage, Preferences)
                VALUES ({0}, {1}, {1}, 'hash', 'UTC', 1, 0, '2026-10-15 00:00:00+00:00', {2}, '{{"TeamDay":null}}');
                """, Guid.NewGuid(), $"user{percentage}", percentage);
        await migrator.MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
        db.ChangeTracker.Clear();
        var rows = await db.UserAttendanceRequirements.ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 37, 100 }, rows.Select(x => x.RequirementValue).Order().ToArray());
        Assert.Equal(3, rows.Select(x => x.Id).Distinct().Count());
        Assert.All(rows, row => { Assert.Equal(DateOnly.MinValue, row.StartingFrom); Assert.NotEqual(default, row.CreatedAt); });
        await migrator.MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(3, await db.UserAttendanceRequirements.CountAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.False(db.Database.HasPendingModelChanges());
        var user = await db.Users.FirstAsync(cancellationToken: TestContext.Current.CancellationToken);
        db.Users.Remove(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, await db.UserAttendanceRequirements.CountAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Current_requirement_uses_the_users_local_month_at_a_utc_boundary()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await SignIn(client);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Username == "history", cancellationToken: TestContext.Current.CancellationToken);
        db.UserAttendanceRequirements.Add(new UserAttendanceRequirement
        {
            Id = Guid.NewGuid(), UserId = user.Id, StartingFrom = new DateOnly(2099, 10, 1),
            RequirementValue = 80, CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dates = new UserDateService(new FixedTimeProvider(new DateTimeOffset(2099, 9, 30, 22, 0, 0, TimeSpan.Zero)));
        Assert.Equal(80, await new OfficeRequirementReader(db, dates).Current(user, CancellationToken.None));
    }
}
