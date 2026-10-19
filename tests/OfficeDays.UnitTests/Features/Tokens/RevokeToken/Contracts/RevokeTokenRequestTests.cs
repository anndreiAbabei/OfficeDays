using OfficeDays.Features.Tokens.RevokeToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.RevokeToken.Contracts;

public sealed class RevokeTokenRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();

        // act
        var sut = new RevokeTokenRequest
        {
            Id = expectedId
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
    }
}
