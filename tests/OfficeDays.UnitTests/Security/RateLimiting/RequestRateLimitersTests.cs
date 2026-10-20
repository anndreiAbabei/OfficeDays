using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OfficeDays.Security.RateLimiting;

namespace OfficeDays.UnitTests.Security.RateLimiting;

public sealed class RequestRateLimitersTests
{
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "Safe!")]
    public async Task Should_AdmitExactlyTheSourceAllowance_WhenRequestsAreConcurrent()
    {
        // Arrange
        const int expectedAllowed = 10;
        var limits = new RequestLimits
        {
            Source = new RequestLimit(expectedAllowed, 60)
        };
        var options = Options.Create(limits);
        using var limiters = new RequestRateLimiters(options);

        // Act
        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            using var lease = limiters.Source.AttemptAcquire(Context());
            return lease.IsAcquired;
        })));

        // Assert
        Assert.Equal(expectedAllowed, results.Count(allowed => allowed));
    }

    [Fact]
    public void Should_ReturnRetryMetadataAndPolicy_WhenSourceAllowanceIsExhausted()
    {
        // Arrange
        const string expectedPolicy = "source";
        const int expectedWindowSeconds = 300;
        var limits = new RequestLimits
        {
            Source = new RequestLimit(1, expectedWindowSeconds)
        };
        var options = Options.Create(limits);
        using var limiters = new RequestRateLimiters(options);
        var context = Context();
        using var first = limiters.Source.AttemptAcquire(context);

        // Act
        using var rejected = limiters.Source.AttemptAcquire(context);
        using var separateSource = limiters.Source.AttemptAcquire(Context("192.0.2.2"));

        // Assert
        Assert.True(first.IsAcquired);
        Assert.False(rejected.IsAcquired);
        Assert.True(rejected.TryGetMetadata(MetadataName.RetryAfter, out var retry));
        Assert.InRange(retry.TotalSeconds, 1, expectedWindowSeconds);
        Assert.Equal(expectedPolicy, context.Items[RequestRateLimiters.PolicyKey]);
        Assert.True(separateSource.IsAcquired);
    }

    [Fact]
    public void Should_ShareUserAllowance_WhenCookiesAndDifferentTokensAreUsed()
    {
        // Arrange
        using var limiters = new RequestRateLimiters(Options.Create(new RequestLimits { Api = new RequestLimit(2, 60) }));
        const string expectedPolicy = "api";
        using var cookie = limiters.Api.AttemptAcquire(Context(user: "one"));
        using var firstToken = limiters.Api.AttemptAcquire(Context(user: "one", token: "first"));
        var secondTokenContext = Context(user: "one", token: "second");

        // Act
        using var secondToken = limiters.Api.AttemptAcquire(secondTokenContext);
        using var otherUser = limiters.Api.AttemptAcquire(Context(user: "two"));

        // Assert
        Assert.True(cookie.IsAcquired);
        Assert.True(firstToken.IsAcquired);
        Assert.False(secondToken.IsAcquired);
        Assert.Equal(expectedPolicy, secondTokenContext.Items[RequestRateLimiters.PolicyKey]);
        Assert.True(otherUser.IsAcquired);
    }

    [Fact]
    public void Should_EnforceTokenAllowance_WhenUserAllowanceRemains()
    {
        // Arrange
        using var limiters = new RequestRateLimiters(Options.Create(new RequestLimits { Token = new RequestLimit(1, 60) }));
        var context = Context(user: "one", token: "first");
        const string expectedPolicy = "token";
        using var first = limiters.Api.AttemptAcquire(context);

        // Act
        using var rejected = limiters.Api.AttemptAcquire(context);
        using var otherToken = limiters.Api.AttemptAcquire(Context(user: "one", token: "second"));

        // Assert
        Assert.True(first.IsAcquired);
        Assert.False(rejected.IsAcquired);
        Assert.Equal(expectedPolicy, context.Items[RequestRateLimiters.PolicyKey]);
        Assert.True(otherToken.IsAcquired);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/_app/asset.js")]
    public void Should_ExemptNonApiPaths_WhenAllowancesAreExhausted(string path)
    {
        // Arrange
        using var limiters = new RequestRateLimiters(Options.Create(new RequestLimits { Source = new RequestLimit(1, 60), Api = new RequestLimit(1, 60) }));
        using var source = limiters.Source.AttemptAcquire(Context());
        using var api = limiters.Api.AttemptAcquire(Context());
        var context = Context();
        context.Request.Path = path;

        // Act
        using var exemptSource = limiters.Source.AttemptAcquire(context);
        using var exemptApi = limiters.Api.AttemptAcquire(context);

        // Assert
        Assert.True(exemptSource.IsAcquired);
        Assert.True(exemptApi.IsAcquired);
    }

    [Theory]
    [InlineData("login")]
    [InlineData("recovery")]
    public void Should_LimitAccountsAcrossSources_WhenSensitivePolicyIsApplied(string policy)
    {
        // Arrange
        using var limiters = new RequestRateLimiters(Options.Create(new RequestLimits
        {
            LoginAccount = new RequestLimit(1, 60), RecoveryAccount = new RequestLimit(1, 60)
        }));
        var first = Context();
        var second = Context("192.0.2.2");
        var endpoint = new Endpoint(null, new EndpointMetadataCollection(new SensitiveRequestLimit(policy, "username")), "sensitive");
        first.SetEndpoint(endpoint);
        second.SetEndpoint(endpoint);
        first.Items[RequestRateLimiters.AccountKey] = "normalized-account-hash";
        second.Items[RequestRateLimiters.AccountKey] = "normalized-account-hash";
        using var allowed = limiters.Api.AttemptAcquire(first);

        // Act
        using var rejected = limiters.Api.AttemptAcquire(second);

        // Assert
        Assert.True(allowed.IsAcquired);
        Assert.False(rejected.IsAcquired);
        Assert.True(rejected.TryGetMetadata(MetadataName.RetryAfter, out _));
    }

    private static DefaultHttpContext Context(string source = "192.0.2.1", string? user = null, string? token = null)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = "/api/auth/me"
            },
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse(source)
            }
        };
        
        if (user is null)
            return context;
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user)
        };
        if (token is not null)
            claims.Add(new Claim("api_token_id", token));

        var identity = new ClaimsIdentity(claims, "test");
        context.User = new ClaimsPrincipal(identity);
        
        return context;
    }
}
