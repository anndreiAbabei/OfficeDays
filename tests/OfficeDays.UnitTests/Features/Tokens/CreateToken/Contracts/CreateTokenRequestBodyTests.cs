using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken.Contracts;

public sealed class CreateTokenRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedName = "Name value";

        // act
        var sut = new CreateTokenRequestBody(expectedName);

        // assert
        Assert.Equal(expectedName, sut.Name);
    }
}
