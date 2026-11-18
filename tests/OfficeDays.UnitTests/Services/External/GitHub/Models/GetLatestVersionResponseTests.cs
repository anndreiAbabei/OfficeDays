using System.Text.Json;
using OfficeDays.Services.External.GitHub.Models;

namespace OfficeDays.UnitTests.Services.External.GitHub.Models;

public sealed class GetLatestVersionResponseTests
{
    [Fact]
    public void Should_ReadReleaseAndAuthor_WhenGitHubReturnsSnakeCaseProperties()
    {
        // Arrange
        const string expectedTag = "2.5.0";
        const string expectedTitle = "November release";
        const string expectedAuthor = "octocat";
        const string expectedNode = "author-node";
        var expectedPublishedAt = new DateTime(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);
        const string payload = """
            {
                "tag_name":"2.5.0", "name":"November release", "published_at":"2026-11-16T12:00:00Z",
                "author":{"login":"octocat","node_id":"author-node","site_admin":true}
            }
            """;

        // Act
        var release = JsonSerializer.Deserialize<GetLatestVersionResponse>(payload);

        // Assert
        Assert.NotNull(release);
        Assert.Equal(expectedTag, release.TagName);
        Assert.Equal(expectedTitle, release.Name);
        Assert.Equal(expectedPublishedAt, release.PublishedAt);
        Assert.Equal(expectedAuthor, release.Author.Login);
        Assert.Equal(expectedNode, release.Author.NodeId);
        Assert.True(release.Author.SiteAdmin);
    }
}
