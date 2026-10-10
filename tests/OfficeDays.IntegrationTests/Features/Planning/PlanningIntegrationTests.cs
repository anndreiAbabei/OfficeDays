using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Planning;

public sealed class PlanningIntegrationTests
{
    private static async Task SignIn(HttpClient client, string name)
    {
        await CreateUser(client, name);
        await Login(client, name, "Normal-user-password!");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await Csrf(client));
    }

    [Fact]
    public async Task Plans_are_idempotent_scoped_to_user_and_month_and_preserved_by_attendance()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await SignIn(client, "planner");
        for (var i = 0; i < 2; i++)
            Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync("/api/planning/2099-09-15", null)).StatusCode);
        var month = await Read(await client.GetAsync("/api/planning?year=2099&month=9"));
        Assert.Equal(1, month.GetProperty("upcomingPlannedDays").GetInt32());
        Assert.Equal(1, (await Read(await client.GetAsync("/api/status?year=2099&month=9"))).GetProperty("upcomingPlannedDays").GetInt32());
        Assert.Equal(0, (await Read(await client.GetAsync("/api/planning?year=2099&month=10"))).GetProperty("upcomingPlannedDays").GetInt32());
        using var other = factory.CreateClient();
        await SignIn(other, "otherplanner");
        Assert.Equal(0, (await Read(await other.GetAsync("/api/planning?year=2099&month=9"))).GetProperty("upcomingPlannedDays").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, (await other.DeleteAsync("/api/planning/2099-09-15")).StatusCode);
        (await client.PutAsync("/api/attendance/2099-09-15", null)).EnsureSuccessStatusCode();
        month = await Read(await client.GetAsync("/api/planning?year=2099&month=9"));
        var day = month.GetProperty("days").EnumerateArray().Single(x => x.GetProperty("date").GetString() == "2099-09-15");
        Assert.True(day.GetProperty("isPlanned").GetBoolean());
        Assert.True(day.GetProperty("isToday").GetBoolean());
        Assert.Equal("attended", day.GetProperty("status").GetString());
        Assert.Equal(0, month.GetProperty("upcomingPlannedDays").GetInt32());
        for (var i = 0; i < 2; i++)
            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/planning/2099-09-15")).StatusCode);
    }

    [Fact]
    public async Task Past_weekend_vacation_and_jurisdiction_holiday_rules_are_enforced()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await SignIn(client, "eligibility");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(x => x.Username == "eligibility");
        var future = new DateOnly(2099, 9, 16);
        db.OfficePlans.Add(new OfficePlan { Id = Guid.NewGuid(), UserId = user.Id, Date = new(2099, 9, 14), CreatedAt = DateTimeOffset.UtcNow });
        db.BankHolidays.Add(new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdictionId = user.HolidayJurisdictionId, Date = new(2099, 9, 17), Name = "Test holiday" });
        await db.SaveChangesAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/2099-09-14", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync("/api/planning/2099-09-14")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/2099-09-19", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/2099-09-17", null)).StatusCode);
        (await client.PutAsync("/api/planning/2099-09-16", null)).EnsureSuccessStatusCode();
        var vacation = await Read(await client.PostAsJsonAsync("/api/vacations", new { from = future, to = future }));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/2099-09-16", null)).StatusCode);
        var month = await Read(await client.GetAsync("/api/planning?year=2099&month=9"));
        var days = month.GetProperty("days").EnumerateArray().ToArray();
        Assert.Equal("missed", days[13].GetProperty("status").GetString());
        Assert.Equal("holiday", days[15].GetProperty("status").GetString());
        Assert.True(days[15].GetProperty("isPlanned").GetBoolean());
        Assert.Equal("Test holiday", days[16].GetProperty("holidays")[0].GetString());
        Assert.Equal(0, month.GetProperty("upcomingPlannedDays").GetInt32());
        Assert.Equal(0, (await Read(await client.GetAsync("/api/status?year=2099&month=9"))).GetProperty("upcomingPlannedDays").GetInt32());
        (await client.DeleteAsync($"/api/vacations/{vacation.GetProperty("id").GetString()}")).EnsureSuccessStatusCode();
        Assert.Equal(1, (await Read(await client.GetAsync("/api/planning?year=2099&month=9"))).GetProperty("upcomingPlannedDays").GetInt32());
        await DeleteUser(factory, "eligibility");
        Assert.Empty(await db.OfficePlans.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Endpoints_require_authentication_csrf_and_valid_dates()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/planning?year=2099&month=9")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PutAsync("/api/planning/2099-09-15", null)).StatusCode);
        await CreateUser(client, "validation");
        await Login(client, "validation", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/2099-09-15", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync("/api/planning/2099-09-15")).StatusCode);
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await Csrf(client));
        foreach (var query in new[] { "", "?year=2099", "?month=9", "?year=0&month=1", "?year=2099&month=13" })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/planning" + query)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsync("/api/planning/not-a-date", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync("/api/planning/not-a-date")).StatusCode);
    }
}
