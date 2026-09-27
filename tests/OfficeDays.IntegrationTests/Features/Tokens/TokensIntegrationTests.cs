using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Tokens;

public sealed class TokensIntegrationTests
{
    [Fact]
    public async Task Bearer_tokens_are_rejected_over_http_when_https_is_required()
    {
        using var factory = new OfficeDaysFactory(requireHttpsForBearerTokens: true);
        using var browser = factory.CreateClient();
        await CreateUser(browser, "secureuser");
        await Login(browser, "secureuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var created = await Read(await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf));

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", created.GetProperty("token").GetString());
        var response = await shortcut.PostAsync("/api/attendance", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("HTTPS required", (await Read(response)).GetProperty("title").GetString());
    }

    [Fact]
    public async Task Token_authenticates_updates_last_use_and_revocation_is_immediate()
    {
        using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "tokenuser");
        await Login(browser, "tokenuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var createdResponse = await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await Read(createdResponse);
        var raw = created.GetProperty("token").GetString()!;
        Assert.StartsWith("odt_", raw);

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", raw);
        Assert.Equal(HttpStatusCode.Created, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
        var tokens = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.NotEqual(JsonValueKind.Null, tokens![0].GetProperty("lastUsedAt").ValueKind);

        var id = created.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(browser, $"/api/tokens/{id}", csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
    }

    [Fact]
    public async Task Users_cannot_manage_other_users_tokens()
    {
        using var factory = new OfficeDaysFactory();
        using var first = factory.CreateClient();
        await CreateUser(first, "firstuser"); await Login(first, "firstuser", "Normal-user-password!");
        var firstCsrf = await Csrf(first);
        var token = await Read(await PostJson(first, "/api/tokens", new { name = "Mine" }, firstCsrf));

        using var second = factory.CreateClient();
        await CreateUser(second, "seconduser"); await Login(second, "seconduser", "Normal-user-password!");
        var secondCsrf = await Csrf(second);
        var response = await Delete(second, $"/api/tokens/{token.GetProperty("id").GetGuid()}", secondCsrf);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty((await second.GetFromJsonAsync<JsonElement[]>("/api/tokens"))!);
    }

}
