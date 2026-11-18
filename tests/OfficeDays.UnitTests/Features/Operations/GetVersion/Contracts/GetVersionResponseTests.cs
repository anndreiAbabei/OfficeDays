using OfficeDays.Features.Operations.GetVersion.Contracts;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion.Contracts;

public sealed class GetVersionResponseTests
{
    [Fact]
    public void Should_SetProperties_WhenConstructed()
    {
        // arrange
        var expectedVersion = "Version value";
        const string expectedLatestVersion = "2.4.0";
        var expectedAuthor = "Author value";
        var expectedGitHubRepo = "GitHubRepo value";

        // act
        var sut = new GetVersionResponse(expectedVersion, expectedLatestVersion, expectedAuthor, expectedGitHubRepo);

        // assert
        Assert.Equal(expectedVersion, sut.Version);
        Assert.Equal(expectedLatestVersion, sut.LatestVersion);
        Assert.Equal(expectedAuthor, sut.Author);
        Assert.Equal(expectedGitHubRepo, sut.GitHubRepo);
    }

    [Theory]
    [InlineData("2.9.0+local", "2.10.0", true)]
    [InlineData("2.4.0+build.20261116", "2.4.0", false)]
    [InlineData("3.0.0", "2.4.0", false)]
    [InlineData("unknown", "2.4.0", false)]
    [InlineData("2.4.0", "unknown", false)]
    public void Should_ReportUpdateAvailability_WhenComparingVersions(string installed, string latest, bool expected)
    {
        // Arrange
        var response = new GetVersionResponse(installed, latest, "author", "repo");

        // Act
        var available = response.NewVersionAvailable;

        // Assert
        Assert.Equal(expected, available);
    }
}
