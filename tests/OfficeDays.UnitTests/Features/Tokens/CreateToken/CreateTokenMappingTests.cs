using OfficeDays.Domain;
using OfficeDays.Features.Tokens.CreateToken;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenMappingTests
{
    [Fact]
    public void Should_MapToCreatedViewModel()
    {
        // arrange
        var expectedToken = "raw-token";
        var sut = new ApiToken { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Desktop", TokenHash = "hash", CreatedAt = new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), LastUsedAt = new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero) };

        // act
        var result = sut.ToCreatedViewModel(expectedToken);

        // assert
        Assert.Equal(sut.Id, result.Id);
        Assert.Equal(sut.Name, result.Name);
        Assert.Equal(sut.CreatedAt, result.CreatedAt);
        Assert.Equal(expectedToken, result.Token);
    }
}
