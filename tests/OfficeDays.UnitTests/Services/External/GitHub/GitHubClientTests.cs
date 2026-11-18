using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeDays.Services.External.GitHub;

namespace OfficeDays.UnitTests.Services.External.GitHub;

public sealed class GitHubClientTests
{
    [Theory]
    [InlineData("""{"name":"Spring release","tag_name":"v2.10.0"}""", "2.10.0")]
    [InlineData("""{"tag_name":"2.4.0"}""", "2.4.0")]
    public async Task Should_ReturnReleaseVersion_WhenTagIsNumeric(string payload, string expectedVersion)
    {
        // Arrange
        using var http = CreateClient(() => new StringContent(payload));
        var client = new GitHubClient(http, NullLogger<GitHubClient>.Instance);

        // Act
        var result = await client.GetLatestVersion(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result is GitHubGetLatestVersionSuccessResponse { Version: var version } && version == expectedVersion);
    }

    [Theory]
    [InlineData("invalid json")]
    [InlineData("null")]
    [InlineData("""{"name":"2.4.0"}""")]
    [InlineData("""{"tag_name":"not-a-version"}""")]
    public async Task Should_ReturnFailure_WhenReleaseIsInvalid(string payload)
    {
        // Arrange
        using var http = CreateClient(() => new StringContent(payload));
        var client = new GitHubClient(http, NullLogger<GitHubClient>.Instance);

        // Act
        var result = await client.GetLatestVersion(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result is GitHubGetLatestVersionErrorResponse { Error.Length: > 0 });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_ReturnFailure_WhenUpstreamIsUnreachable(bool timeout)
    {
        // Arrange
        using var http = CreateClient(() => throw (timeout ? new TaskCanceledException() : new HttpRequestException()));
        var client = new GitHubClient(http, NullLogger<GitHubClient>.Instance);

        // Act
        var result = await client.GetLatestVersion(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result is GitHubGetLatestVersionErrorResponse { Error.Length: > 0 });
    }

    private static HttpClient CreateClient(Func<HttpContent> content) =>
        new HttpClient(new ReleaseHandler(content)) { BaseAddress = new Uri("https://api.github.com/repos/example/OfficeDays/") };

    private sealed class ReleaseHandler(Func<HttpContent> content) : HttpMessageHandler
    {
        private readonly Func<HttpContent> _content = content;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = _content() });
    }
}
