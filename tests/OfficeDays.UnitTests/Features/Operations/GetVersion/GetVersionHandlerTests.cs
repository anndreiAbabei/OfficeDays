using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OfficeDays.UnitTests.Testing;
using OfficeDays.Services.External.GitHub;
using NSubstitute;
using OfficeDays.Features.Operations.GetVersion;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion;

public sealed class GetVersionHandlerTests : IDisposable
{
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IGitHubClient _github = Substitute.For<IGitHubClient>();

    private readonly FakeLogger<GetVersionHandler> _logger = new FakeLogger<GetVersionHandler>();

    [Fact]
    public async Task Should_CacheReleaseAndDisableHttpCaching_WhenReadingVersionTwice()
    {
        // arrange
        var context = new DefaultHttpContext();
        var appVersion = Substitute.For<IAppVersionService>();
        const string expectedVersion = "3.0.0+test-build";
        const string expectedLatest = "3.1.0";
        const string expectedCacheControl = "no-store";
        appVersion.GetAll().Returns(new AppVersionInfo(expectedVersion, "author", "repo"));
        GitHubGetLatestVersionResponse latest = new GitHubGetLatestVersionSuccessResponse(expectedLatest);
        _github.GetLatestVersion(Arg.Any<CancellationToken>()).Returns(latest);
        var sut = CreateHandler(new HttpContextAccessor { HttpContext = context }, appVersion);

        // act
        var result = await sut.Handle(new GetVersionRequest(), CancellationToken.None);

        await sut.Handle(new GetVersionRequest(), CancellationToken.None);

        // assert
        var response = Assert.IsType<Ok<GetVersionResponse>>(result);
        Assert.Equal(expectedVersion, response.Value!.Version);
        Assert.Equal(expectedLatest, response.Value.LatestVersion);
        const int expectedEventId = 1401;
        var log = Assert.Single(_logger.Logs);
        Assert.Equal(LogLevel.Information, log.Level);
        Assert.Equal(expectedEventId, log.EventId.Id);
        Assert.Equal(new GitHubGetLatestVersionSuccessResponse(expectedLatest), log.Values["Response"]);
        await _github.Received(1).GetLatestVersion(Arg.Any<CancellationToken>());
        Assert.Equal(expectedCacheControl, context.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task Should_Throw_WhenCancelled()
    {
        // arrange
        var sut = CreateHandler(new HttpContextAccessor(), Substitute.For<IAppVersionService>());

        // act & assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(new GetVersionRequest(), new CancellationToken(true)).AsTask());
    }

    [Fact]
    public async Task Should_Throw_WhenHttpContextIsMissing()
    {
        // arrange
        var sut = CreateHandler(new HttpContextAccessor(), Substitute.For<IAppVersionService>());

        // act & assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(new GetVersionRequest(), CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Should_ExpireFallback_WhenGitHubCheckFails()
    {
        // Arrange
        const string expectedVersion = "2.4.0+local";
        const int expectedEventId = 1402;
        var expectedRetryDelay = TimeSpan.FromMinutes(5);
        var error = new GitHubGetLatestVersionErrorResponse("Service unavailable");
        var appVersion = Substitute.For<IAppVersionService>();
        appVersion.Version.Returns(expectedVersion);
        appVersion.GetAll().Returns(new AppVersionInfo(expectedVersion, "author", "repo"));
        _github.GetLatestVersion(Arg.Any<CancellationToken>()).Returns((GitHubGetLatestVersionResponse)error);
        var entry = Substitute.For<ICacheEntry>();
        var cache = Substitute.For<IMemoryCache>();
        cache.CreateEntry(Arg.Any<object>()).Returns(entry);
        var context = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var handler = new GetVersionHandler(context, appVersion, cache, _github, TimeProvider.System, _logger);

        // Act
        var result = await handler.Handle(new GetVersionRequest(), TestContext.Current.CancellationToken);

        // Assert
        var response = Assert.IsType<Ok<GetVersionResponse>>(result);
        Assert.Equal(expectedVersion, response.Value!.LatestVersion);
        Assert.False(response.Value.NewVersionAvailable);
        Assert.Equal(expectedRetryDelay, entry.AbsoluteExpirationRelativeToNow);
        var log = Assert.Single(_logger.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Equal(expectedEventId, log.EventId.Id);
        Assert.Same(error, log.Values["error"]);
    }

    public void Dispose() => _cache.Dispose();

    private GetVersionHandler CreateHandler(IHttpContextAccessor accessor, IAppVersionService version) =>
        new GetVersionHandler(accessor, version, _cache, _github, TimeProvider.System, _logger);
}
