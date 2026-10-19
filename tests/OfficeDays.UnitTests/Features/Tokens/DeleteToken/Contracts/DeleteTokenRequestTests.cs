using OfficeDays.Features.Tokens.DeleteToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.DeleteToken.Contracts;

public sealed class DeleteTokenRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();

        // act
        var sut = new DeleteTokenRequest
        {
            Id = expectedId
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
    }
}
