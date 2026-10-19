using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken.Contracts;

public sealed class CreateTokenRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new CreateTokenRequestBody("Name value");

        // act
        var sut = new CreateTokenRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
