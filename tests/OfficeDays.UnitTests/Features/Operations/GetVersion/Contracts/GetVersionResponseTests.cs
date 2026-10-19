using OfficeDays.Features.Operations.GetVersion.Contracts;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion.Contracts;

public sealed class GetVersionResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedVersion = "Version value";
        var expectedAuthor = "Author value";
        var expectedGitHubRepo = "GitHubRepo value";

        // act
        var sut = new GetVersionResponse(expectedVersion, expectedAuthor, expectedGitHubRepo);

        // assert
        Assert.Equal(expectedVersion, sut.Version);
        Assert.Equal(expectedAuthor, sut.Author);
        Assert.Equal(expectedGitHubRepo, sut.GitHubRepo);
    }
}
