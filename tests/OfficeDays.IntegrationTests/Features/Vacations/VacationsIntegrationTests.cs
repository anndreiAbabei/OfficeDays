using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Vacations;

public sealed class VacationsIntegrationTests
{
    [Fact]
    public async Task Vacation_creation_and_removal_are_scoped_to_the_user()
    {
        using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationer"); await Login(owner, "vacationer", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));

        using var other = factory.CreateClient();
        await CreateUser(other, "outsider"); await Login(other, "outsider", "Normal-user-password!");
        var otherCsrf = await Csrf(other);
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(other, $"/api/vacations/{created.GetProperty("id").GetGuid()}", otherCsrf)).StatusCode);
        Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9"))!);
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", csrf)).StatusCode);
    }

    [Fact]
    public async Task Vacation_endpoints_reject_invalid_ranges_and_filters_and_report_missing_records()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "vacationfailures");
        await Login(client, "vacationfailures", "Normal-user-password!");
        var csrf = await Csrf(client);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await PostJson(client, "/api/vacations", new { from = "2026-09-25", to = "2026-09-21" }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/vacations?month=9")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await Delete(client, $"/api/vacations/{Guid.NewGuid()}", csrf)).StatusCode);
    }

}
