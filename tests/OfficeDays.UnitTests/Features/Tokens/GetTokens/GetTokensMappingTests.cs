using OfficeDays.Domain;
using OfficeDays.Features.Tokens.GetTokens;

namespace OfficeDays.UnitTests.Features.Tokens.GetTokens;

public sealed class GetTokensMappingTests
{
    [Theory]
    [InlineData(false, "Active")]
    [InlineData(true, "Revoked")]
    public void Should_MapTokenStatus(bool revoked, string expectedStatus)
    {
        // arrange
        var sut = new ApiToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Desktop", TokenHash = "hash", CreatedAt = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), LastUsedAt = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero) };
        sut.RevokedAt = revoked ? new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero) : null;

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.Name, result.Name);
        Assert.Equal(sut.CreatedAt, result.CreatedAt);
        Assert.Equal(sut.LastUsedAt, result.LastUsedAt);
        Assert.Equal(sut.RevokedAt, result.RevokedAt);
        Assert.Equal(expectedStatus, result.Status);
    }

    [Fact]
    public void Should_PreserveNullLastUsedAt()
    {
        // arrange
        var sut = new ApiToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Desktop", TokenHash = "hash", CreatedAt = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), LastUsedAt = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero) };
        sut.LastUsedAt = null;

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Null(result.LastUsedAt);
    }

    [Fact]
    public void Should_MapCollectionInSourceOrder()
    {
        // arrange
        var first = new ApiToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Desktop", TokenHash = "hash", CreatedAt = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), LastUsedAt = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero) };
        var second = new ApiToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Phone", TokenHash = "hash", CreatedAt = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), LastUsedAt = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero) };
        ApiToken[] sut = [first, second];

        // act
        var result = sut.ToViewModel().ToArray();

        // assert
        Assert.Equal(new[] { first.Id, second.Id }, result.Select(item => item.Id));
        Assert.Equal(new[] { "Desktop", "Phone" }, result.Select(item => item.Name));
    }

    [Fact]
    public void Should_MapEmptyCollection()
    {
        // arrange
        ApiToken[] sut = [];

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Empty(result);
    }
}
