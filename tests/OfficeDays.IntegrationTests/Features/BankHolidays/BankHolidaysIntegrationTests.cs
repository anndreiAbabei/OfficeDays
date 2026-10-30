using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.BankHolidays;

public sealed class BankHolidaysIntegrationTests
{
    [Fact]
    public async Task Admin_can_remove_the_last_holiday_without_affecting_other_years()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await Login(client, "Admin", "VeryStrongAdminPassword!");
        var csrf = await Csrf(client);
        await PutJson(client, "/api/bank-holidays/RO/2026",
            new[] { new { date = "2026-01-01", name = "New Year" } }, csrf);
        await PutJson(client, "/api/bank-holidays/RO/2027",
            new[] { new { date = "2027-01-01", name = "New Year" } }, csrf);
        var response = await PutJson(client, "/api/bank-holidays/RO/2026", Array.Empty<object>(), csrf);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Single((await client.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2027", cancellationToken: TestContext.Current.CancellationToken))!);
    }

    [Fact]
    public async Task Admin_can_replace_holidays_but_normal_user_cannot()
    {
        await using var factory = new OfficeDaysFactory();
        using var normal = factory.CreateClient();
        await CreateUser(normal, "normal");
        await Login(normal, "normal", "Normal-user-password!");
        var normalCsrf = await Csrf(normal);
        var denied = await PutJson(normal, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-01-01", name = "New Year" } }, normalCsrf);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        var first = await PutJson(admin, "/api/bank-holidays/RO/2026", new[]
        {
            new { date = "2026-01-01", name = "New Year" }, new { date = "2026-12-25", name = "Christmas" }
        }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var second = await PutJson(admin, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-05-01", name = "Labour Day" } }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var holidays = await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(holidays!);
        Assert.Equal("2026-05-01", holidays![0].GetProperty("date").GetString());

        await PostJson(admin, "/api/holiday-jurisdictions",
            new { code = "GB-NIR", name = "Northern Ireland" }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK,
            (await PutJson(admin, "/api/bank-holidays/GB-NIR/2026",
                new[] { new { date = "2026-05-01", name = "Northern Ireland holiday" } }, adminCsrf)).StatusCode);
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/GB-NIR/2026", cancellationToken: TestContext.Current.CancellationToken))!);
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026", cancellationToken: TestContext.Current.CancellationToken))!);
    }

    [Fact]
    public async Task Bank_holiday_endpoints_reject_invalid_years_and_replacement_payloads()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await Login(client, "Admin", "VeryStrongAdminPassword!");
        var csrf = await Csrf(client);

        var seed = await PutJson(client, "/api/bank-holidays/RO/2026",
            new[] { new { date = "2026-05-01", name = "Preserved holiday" } }, csrf);
        Assert.Equal(HttpStatusCode.OK, seed.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/bank-holidays/RO/0", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026",
                new[] { new { date = "2025-12-31", name = "Wrong year" } }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026",
                new[] { new { date = "2026-01-01", name = "" } }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026", new[]
            {
                new { date = "2026-01-01", name = "First" },
                new { date = "2026-01-01", name = "Duplicate" }
            }, csrf)).StatusCode);

        var nullPayload = new HttpRequestMessage(HttpMethod.Put, "/api/bank-holidays/RO/2026");
        nullPayload.Headers.Add("X-CSRF-TOKEN", csrf);
        nullPayload.Content = JsonContent.Create<List<object>?>(null);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(nullPayload, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026", new object?[] { null }, csrf)).StatusCode);
        var holidays = await client.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("2026-05-01", Assert.Single(holidays!).GetProperty("date").GetString());
    }

}
