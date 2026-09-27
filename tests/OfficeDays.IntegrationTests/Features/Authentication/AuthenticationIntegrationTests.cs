using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Authentication;

public sealed class AuthenticationIntegrationTests
{
    [Fact]
    public async Task Login_cookie_is_persistent()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "persistentuser");

        var response = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "persistentuser", password = "Normal-user-password!" });

        response.EnsureSuccessStatusCode();
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith("OfficeDays.Session=", StringComparison.Ordinal));
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_rejects_unknown_users_and_wrong_passwords_and_logout_ends_the_session()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "loginbranches");

        var unknown = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "does-not-exist", password = "Normal-user-password!" });
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "loginbranches", password = "Incorrect-password!" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var missingCredentials = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, missingCredentials.StatusCode);

        await Login(client, "loginbranches", "Normal-user-password!");
        var csrf = await Csrf(client);
        var logout = await PostJson(client, "/api/auth/logout", new { }, csrf);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

}
