using OfficeDays.Features.Tokens.GetTokens.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.GetTokens.Contracts;

public sealed class GetTokensItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedName = "Name value";
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 3, 9, 30, 0, TimeSpan.Zero);
        var expectedLastUsedAt = new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero);
        var expectedRevokedAt = new DateTimeOffset(2026, 10, 5, 9, 30, 0, TimeSpan.Zero);
        var expectedStatus = "Active";

        // act
        var sut = new GetTokensItem(expectedId, expectedName, expectedCreatedAt, expectedLastUsedAt, expectedRevokedAt, expectedStatus);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Equal(expectedLastUsedAt, sut.LastUsedAt);
        Assert.Equal(expectedRevokedAt, sut.RevokedAt);
        Assert.Equal(expectedStatus, sut.Status);
    }
}
