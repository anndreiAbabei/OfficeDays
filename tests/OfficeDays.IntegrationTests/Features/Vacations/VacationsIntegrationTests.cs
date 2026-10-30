using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Vacations;

public sealed class VacationsIntegrationTests
{
    [Fact]
    public async Task Vacation_creation_and_removal_are_scoped_to_the_user()
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationer"); await Login(owner, "vacationer", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));

        using var other = factory.CreateClient();
        await CreateUser(other, "outsider"); await Login(other, "outsider", "Normal-user-password!");
        var otherCsrf = await Csrf(other);
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(other, $"/api/vacations/{created.GetProperty("id").GetGuid()}", otherCsrf)).StatusCode);
        Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", csrf)).StatusCode);
    }

    [Fact]
    public async Task Vacation_endpoints_reject_invalid_ranges_and_filters_and_report_missing_records()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "vacationfailures");
        await Login(client, "vacationfailures", "Normal-user-password!");
        var csrf = await Csrf(client);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await PostJson(client, "/api/vacations", new { from = "2026-09-25", to = "2026-09-21" }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/vacations?month=9", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await Delete(client, $"/api/vacations/{Guid.NewGuid()}", csrf)).StatusCode);
    }

    [Fact]
    public async Task Update_persists_dates_across_months_and_preserves_identity_and_creation_time()
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationedit");
        await Login(owner, "vacationedit", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));
        var id = created.GetProperty("id").GetGuid();
        var originalCreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Vacations.Where(x => x.Id == id).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, originalCreatedAt), cancellationToken: TestContext.Current.CancellationToken);
        var before = await db.Vacations.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent,
            (await PutJson(owner, $"/api/vacations/{id}", new { from = "2026-10-05", to = "2026-10-05" }, csrf)).StatusCode);
        var after = await db.Vacations.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(before.Id, after.Id);
        Assert.Equal(before.UserId, after.UserId);
        Assert.Equal(originalCreatedAt, after.CreatedAt);
        Assert.Equal(new DateOnly(2026, 10, 5), after.From);
        Assert.Equal(after.From, after.To);
        Assert.Equal(1, await db.Vacations.CountAsync(cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken))!);
        var october = Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=10", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Equal(id, october.GetProperty("id").GetGuid());
        Assert.Equal(originalCreatedAt, october.GetProperty("createdAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task Update_rejects_nonowners_missing_records_and_missing_antiforgery()
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationowner");
        await Login(owner, "vacationowner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));
        var path = $"/api/vacations/{created.GetProperty("id").GetGuid()}";
        var dates = new { from = "2026-09-22", to = "2026-09-24" };
        using var other = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.PutAsJsonAsync(path, dates, cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        await CreateUser(other, "vacationoutsider");
        await Login(other, "vacationoutsider", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.NotFound, (await PutJson(other, path, dates, await Csrf(other))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.PutAsJsonAsync(path, dates, cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await PutJson(owner, $"/api/vacations/{Guid.NewGuid()}", dates, csrf)).StatusCode);
        var unchanged = Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Equal("2026-09-21", unchanged.GetProperty("from").GetString());
        Assert.Equal("2026-09-25", unchanged.GetProperty("to").GetString());
    }

    [Theory]
    [InlineData("2026-09-25", "2026-09-21")]
    [InlineData("invalid", "2026-09-25")]
    [InlineData("2026-09-21", "2026-02-30")]
    public async Task Update_rejects_invalid_dates_without_changing_vacation(string from, string to)
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "invalidvacationedit");
        await Login(owner, "invalidvacationedit", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", new { from, to }, csrf)).StatusCode);
        var unchanged = Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Equal("2026-09-21", unchanged.GetProperty("from").GetString());
        Assert.Equal("2026-09-25", unchanged.GetProperty("to").GetString());
    }

    [Fact]
    public async Task Updated_overlapping_ranges_remain_allowed_and_are_counted_once()
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationoverlap");
        await Login(owner, "vacationoverlap", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-23" }, csrf));
        (await PostJson(owner, "/api/vacations", new { from = "2026-09-23", to = "2026-09-25" }, csrf)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent,
            (await PutJson(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", new { from = "2026-09-21", to = "2026-09-24" }, csrf)).StatusCode);
        Assert.Equal(2, (await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9", cancellationToken: TestContext.Current.CancellationToken))!.Length);
        var status = await Read(await owner.GetAsync("/api/status?year=2026&month=9", TestContext.Current.CancellationToken));
        Assert.Equal(17, status.GetProperty("eligibleWorkingDays").GetInt32());
    }

    [Fact]
    public async Task Updated_dates_refresh_calendar_eligibility_and_planned_and_summary_counts()
    {
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationplanning");
        await Login(owner, "vacationplanning", "Normal-user-password!");
        var csrf = await Csrf(owner);
        (await PutJson(owner, "/api/planning/2099-09-16", new { }, csrf)).EnsureSuccessStatusCode();
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2099-09-16", to = "2099-09-16" }, csrf));
        var before = await Read(await owner.GetAsync("/api/planning?year=2099&month=9", TestContext.Current.CancellationToken));
        Assert.Equal(0, before.GetProperty("upcomingPlannedDays").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent,
            (await PutJson(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", new { from = "2099-09-17", to = "2099-09-17" }, csrf)).StatusCode);
        var after = await Read(await owner.GetAsync("/api/planning?year=2099&month=9", TestContext.Current.CancellationToken));
        var days = after.GetProperty("days").EnumerateArray().ToArray();
        Assert.False(days[15].GetProperty("isVacation").GetBoolean());
        Assert.True(days[15].GetProperty("isPlanned").GetBoolean());
        Assert.Equal("planned", days[15].GetProperty("status").GetString());
        Assert.True(days[16].GetProperty("isVacation").GetBoolean());
        Assert.Equal("holiday", days[16].GetProperty("status").GetString());
        Assert.Equal(1, after.GetProperty("upcomingPlannedDays").GetInt32());
        var status = await Read(await owner.GetAsync("/api/status?year=2099&month=9", TestContext.Current.CancellationToken));
        Assert.Equal(1, status.GetProperty("upcomingPlannedDays").GetInt32());
    }

}
