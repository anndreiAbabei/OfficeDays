using System.Net;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Authentication;

public sealed class AuthenticationIntegrationTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Should_SetCookieExpiryOnlyOnRequest_WhenSigningIn(bool? rememberMe, bool expectedPersistence)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        const string username = "persistentuser";
        const string password = "Normal-user-password!";
        await CreateUser(client, username);
        object body = rememberMe.HasValue ? new { username, password, rememberMe } : new { username, password };

        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", body, TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"),
                                   value => value.StartsWith("OfficeDays.Session=", StringComparison.Ordinal));
        Assert.Equal(expectedPersistence, cookie.Contains("expires=", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Login_rejects_unknown_users_and_wrong_passwords_and_logout_ends_the_session()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "loginbranches");

        var unknown = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "does-not-exist", password = "Normal-user-password!" }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "loginbranches", password = "Incorrect-password!" }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var missingCredentials = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "", password = "" }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, missingCredentials.StatusCode);

        await Login(client, "loginbranches", "Normal-user-password!");
        var csrf = await Csrf(client);
        var logout = await PostJson(client, "/api/auth/logout", new { }, csrf);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
    }

}
