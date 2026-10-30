using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Security;

public sealed class RequestLimitingTests
{
    [Fact]
    public async Task Should_LimitNormalizedAccount_WhenLoginThresholdIsReached()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:LoginAccount:Permits"] = "1" })));
        using var client = app.CreateClient();
        const HttpStatusCode expectedStatus = HttpStatusCode.TooManyRequests;

        // Act
        await client.PostAsJsonAsync("/api/auth/login", new { username = "Missing", password = "wrong" }, cancellationToken: TestContext.Current.CancellationToken);
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = " missing ", password = "wrong" }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.NotNull(response.Headers.RetryAfter?.Delta);
        Assert.Contains("try again", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_IgnoreSpoofedForwardingHeaders_WhenSourceLimitIsReached()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:Source:Permits"] = "1" })));
        using var client = app.CreateClient();
        const HttpStatusCode expectedStatus = HttpStatusCode.TooManyRequests;
        await client.GetAsync("/api/auth/csrf", TestContext.Current.CancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/csrf");
        request.Headers.Add("X-Forwarded-For", "1.2.3.4");
        request.Headers.Add("X-Forwarded-Proto", "https");

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
        const HttpStatusCode expectedHealthStatus = HttpStatusCode.OK;
        Assert.Equal(expectedHealthStatus, (await client.GetAsync("/health/live", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_RejectOversizedBody_WhenLoginIsPosted()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        const HttpStatusCode expectedStatus = HttpStatusCode.RequestEntityTooLarge;

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = "test", password = new string('a', 65_536) }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepUserAllowancesSeparate_WhenCookiesShareASource()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:Api:Permits"] = "4" })));
        using var first = app.CreateClient();
        using var second = app.CreateClient();
        await CreateUser(first, "first");
        await Login(first, "first", "Normal-user-password!");
        await CreateUser(second, "second");
        await Login(second, "second", "Normal-user-password!");
        for (var i = 0; i < 4; i++) await first.GetAsync("/api/auth/me", TestContext.Current.CancellationToken);
        const HttpStatusCode expectedLimited = HttpStatusCode.TooManyRequests;
        const HttpStatusCode expectedAllowed = HttpStatusCode.OK;

        // Act / Assert
        Assert.Equal(expectedLimited, (await first.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedAllowed, (await second.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
    }
    [Fact]
    public async Task Should_EnforceTokenAndSharedUserLimits_WhenBearerTokensAreRotated()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?>
                                                                                                          {
                                                                                                              ["RateLimiting:Api:Permits"] = "10",
                                                                                                              ["RateLimiting:Token:Permits"] = "2"
                                                                                                          })));
        using var browser = app.CreateClient();
        await CreateUser(browser, "tokens");
        await Login(browser, "tokens", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var firstToken = await Read(await PostJson(browser, "/api/tokens", new { name = "first" }, csrf));
        var secondToken = await Read(await PostJson(browser, "/api/tokens", new { name = "second" }, csrf));
        using var first = app.CreateClient();
        using var second = app.CreateClient();
        first.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken.GetProperty("token").GetString());
        second.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondToken.GetProperty("token").GetString());
        const HttpStatusCode expectedAllowed = HttpStatusCode.OK;
        const HttpStatusCode expectedLimited = HttpStatusCode.TooManyRequests;

        // Act / Assert
        for (var i = 0; i < 2; i++)
            Assert.Equal(expectedAllowed, (await first.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedLimited, (await first.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        for (var i = 0; i < 2; i++)
            Assert.Equal(expectedAllowed, (await second.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        // ASP.NET retries a failed acquisition through AcquireAsync, even with QueueLimit = 0.
        // The rejected token request therefore consumes two permits from the earlier user limiter:
        // three setup requests + four successful bearer requests + two rejected-attempt permits = nine.
        Assert.Equal(expectedAllowed, (await browser.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedLimited, (await browser.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Theory]
    [InlineData("Registration", "/api/users")]
    [InlineData("Login", "/api/auth/login")]
    public async Task Should_EnforceSensitiveSourceLimit_WhenDifferentAccountsAreTargeted(string policy, string path)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?> { [$"RateLimiting:{policy}:Permits"] = "1" })));
        using var client = app.CreateClient();
        const HttpStatusCode expectedStatus = HttpStatusCode.TooManyRequests;

        // Act
        await client.PostAsJsonAsync(path, new { username = "one", password = "wrong" }, cancellationToken: TestContext.Current.CancellationToken);
        var response = await client.PostAsJsonAsync(path, new { username = "two", password = "wrong" }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData("10.0.0.1", 200)]
    [InlineData("10.0.0.2", 429)]
    public async Task Should_TrustOnlyConfiguredProxy_WhenForwardedSourcesDiffer(string trustedProxy, int expectedStatus)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?>
                                                                                                          {
                                                                                                              ["RateLimiting:Source:Permits"] = "1",
                                                                                                              ["ReverseProxy:KnownProxies:0"] = trustedProxy
                                                                                                          })));
        using var client = app.CreateClient();
        await app.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            context.Request.Path = "/api/auth/csrf";
            context.Request.Headers["X-Forwarded-For"] = "192.0.2.1";
        }, TestContext.Current.CancellationToken);

        // Act
        var response = await app.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            context.Request.Path = "/api/auth/csrf";
            context.Request.Headers["X-Forwarded-For"] = "192.0.2.2";
        }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.Response.StatusCode);
    }

    [Fact]
    public async Task Should_LimitReportsSeparately_WhenApiAllowanceRemains()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:Reports:Permits"] = "1" })));
        using var client = app.CreateClient();
        await Login(client, "admin", "VeryStrongAdminPassword!");
        const HttpStatusCode expectedAllowed = HttpStatusCode.OK;
        const HttpStatusCode expectedLimited = HttpStatusCode.TooManyRequests;

        // Act / Assert
        Assert.Equal(expectedAllowed, (await client.GetAsync("/api/reports", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedLimited, (await client.GetAsync("/api/reports", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedAllowed, (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Theory]
    [InlineData(65_536, 200)]
    [InlineData(65_537, 413)]
    public async Task Should_BoundStreamedBodies_WhenContentLengthIsAbsent(int bodyLength, int expectedStatus)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        using var body = new MemoryStream(new byte[bodyLength]);

        // Act
        var response = await factory.Server.SendAsync(context =>
        {
            context.Request.Path = "/api/auth/csrf";
            context.Request.Body = body;
            context.Request.ContentLength = null;
        }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.Response.StatusCode);
    }

    [Fact]
    public async Task Should_ShareAccountLimitAcrossTrustedSources_WhenCasingChanges()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        await using var app = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                                                                                                          configuration.AddInMemoryCollection(new Dictionary<string, string?>
                                                                                                          {
                                                                                                              ["RateLimiting:LoginAccount:Permits"] = "1",
                                                                                                              ["ReverseProxy:KnownProxies:0"] = "10.0.0.1"
                                                                                                          })));
        using var client = app.CreateClient();
        const int expectedStatus = 429;
        using var firstBody = new MemoryStream([.. "{\"username\":\"missing\",\"password\":\"wrong\"}"u8 ]);
        using var secondBody = new MemoryStream([.. "{\"USERNAME\":\" MISSING \",\"password\":\"wrong\"}"u8 ]);
        await app.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            context.Request.Headers["X-Forwarded-For"] = "192.0.2.1";
            context.Request.Method = "POST";
            context.Request.Path = "/api/auth/login";
            context.Request.ContentType = "application/json";
            context.Request.ContentLength = firstBody.Length;
            context.Request.Body = firstBody;
        }, TestContext.Current.CancellationToken);

        // Act
        var response = await app.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            context.Request.Headers["X-Forwarded-For"] = "192.0.2.2";
            context.Request.Method = "POST";
            context.Request.Path = "/api/auth/login";
            context.Request.ContentType = "application/json";
            context.Request.ContentLength = secondBody.Length;
            context.Request.Body = secondBody;
        }, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.Response.StatusCode);
    }
}
