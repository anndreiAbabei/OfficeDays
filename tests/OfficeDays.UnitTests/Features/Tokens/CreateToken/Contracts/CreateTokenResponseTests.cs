using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken.Contracts;

public sealed class CreateTokenResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedName = "Name value";
        var expectedToken = "Token value";
        var expectedCreatedAt = new DateTimeOffset(2026, 10, 4, 9, 30, 0, TimeSpan.Zero);

        // act
        var sut = new CreateTokenResponse(expectedId, expectedName, expectedToken, expectedCreatedAt);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedToken, sut.Token);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
