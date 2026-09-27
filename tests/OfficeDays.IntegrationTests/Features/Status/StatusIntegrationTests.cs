using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Status;

public sealed class StatusIntegrationTests
{
    [Fact]
    public async Task Status_uses_persisted_holidays_vacations_and_attendance()
    {
        using var factory = new OfficeDaysFactory();
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

        var status = await user.GetFromJsonAsync<JsonElement>("/api/status?year=2026&month=9");
        Assert.Equal(16, status.GetProperty("eligibleWorkingDays").GetInt32()); // 22 weekdays - 1 holiday - 5 vacation
        Assert.Equal(8, status.GetProperty("maximumWfhDays").GetInt32());
        Assert.Equal(8, status.GetProperty("requiredOfficeDays").GetInt32());
        Assert.Equal(2, status.GetProperty("officeDays").GetInt32());
        Assert.Equal(6, status.GetProperty("remainingOfficeDays").GetInt32());
    }

    [Fact]
    public async Task Status_defaults_to_the_users_current_month_and_rejects_incomplete_periods()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "statusdefaults");
        await Login(client, "statusdefaults", "Normal-user-password!");

        var current = await client.GetAsync("/api/status");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal("2099-09", (await Read(current)).GetProperty("period").GetString());

        var incomplete = await client.GetAsync("/api/status?year=2099");
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        foreach (var month in new[] { 0, 13, 9998 })
        {
            var invalid = await client.GetAsync($"/api/status?year=2099&month={month}");
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            Assert.True((await Read(invalid)).GetProperty("errors").TryGetProperty("Month", out _));
        }

        await DeleteUser(factory, "statusdefaults");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/status")).StatusCode);
    }

}
