using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rename_persists_trimmed_name_and_preserves_credential_and_metadata(bool revoked)
    {
        using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "renameuser");
        await Login(browser, "renameuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var created = await Read(await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf));
        var id = created.GetProperty("id").GetGuid();
        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", created.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.Created, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
        if (revoked)
            Assert.Equal(HttpStatusCode.NoContent, (await Delete(browser, $"/api/tokens/{id}", csrf)).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var before = await db.ApiTokens.AsNoTracking().SingleAsync(x => x.Id == id);
        var response = await SendJson(browser, HttpMethod.Patch, $"/api/tokens/{id}", new { name = "  Work phone  " }, csrf);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var after = await db.ApiTokens.AsNoTracking().SingleAsync(x => x.Id == id);
        Assert.Equal("Work phone", after.Name);
        Assert.Equal(before.TokenHash, after.TokenHash);
        Assert.Equal(before.UserId, after.UserId);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.Equal(before.LastUsedAt, after.LastUsedAt);
        Assert.Equal(before.RevokedAt, after.RevokedAt);
        var listed = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.Equal("Work phone", Assert.Single(listed!).GetProperty("name").GetString());
        var credentialResponse = await shortcut.PostAsync("/api/attendance", null);
        Assert.Equal(revoked ? HttpStatusCode.Unauthorized : HttpStatusCode.OK, credentialResponse.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-long")]
    public async Task Rename_rejects_invalid_names_without_changing_token(string? name)
    {
        using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "invalidrename");
        await Login(browser, "invalidrename", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var created = await Read(await PostJson(browser, "/api/tokens", new { name = "Original" }, csrf));
        var id = created.GetProperty("id").GetGuid();
        if (name == "too-long") name = new string('x', 101);
        var response = await SendJson(browser, HttpMethod.Patch, $"/api/tokens/{id}", new { name }, csrf);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var listed = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.Equal("Original", Assert.Single(listed!).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Rename_requires_owner_and_antiforgery_token()
    {
        using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "renameowner");
        await Login(owner, "renameowner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/tokens", new { name = "Original" }, csrf));
        var path = $"/api/tokens/{created.GetProperty("id").GetGuid()}";
        using var other = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.PatchAsJsonAsync(path, new { name = "Stolen" })).StatusCode);
        await CreateUser(other, "renameother");
        await Login(other, "renameother", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.NotFound,
            (await SendJson(other, HttpMethod.Patch, path, new { name = "Stolen" }, await Csrf(other))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await owner.PatchAsJsonAsync(path, new { name = "No CSRF" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await SendJson(owner, HttpMethod.Patch, $"/api/tokens/{Guid.NewGuid()}", new { name = "Missing" }, csrf)).StatusCode);
        var unchanged = await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.Equal("Original", Assert.Single(unchanged!).GetProperty("name").GetString());
        Assert.Equal(HttpStatusCode.NoContent,
            (await SendJson(owner, HttpMethod.Patch, path, new { name = new string('x', 100) }, csrf)).StatusCode);
        var listed = await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.Equal(new string('x', 100), Assert.Single(listed!).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Permanent_delete_requires_revocation_and_removes_record_without_restoring_access()
    {
        using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "deleteowner");
        await Login(owner, "deleteowner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/tokens", new { name = "Phone" }, csrf));
        var id = created.GetProperty("id").GetGuid();
        var path = $"/api/tokens/{id}";
        Assert.Equal(HttpStatusCode.Conflict, (await Delete(owner, path + "/permanent", csrf)).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var active = await db.ApiTokens.SingleAsync(x => x.Id == id);
            Assert.Null(active.RevokedAt);
        }
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(owner, path, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.DeleteAsync(path + "/permanent")).StatusCode);

        using var other = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await other.DeleteAsync(path + "/permanent")).StatusCode);
        await CreateUser(other, "deleteother");
        await Login(other, "deleteother", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(other, path + "/permanent", await Csrf(other))).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.True(await db.ApiTokens.AnyAsync(x => x.Id == id));
        }
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(owner, path + "/permanent", csrf)).StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.False(await db.ApiTokens.AnyAsync(x => x.Id == id));
        }
        Assert.Empty((await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(owner, path + "/permanent", csrf)).StatusCode);
        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", created.GetProperty("token").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
    }

}
