using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.Login.Contracts;

public sealed class LoginRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedBody = new LoginRequestBody("Username value", "a-long-password");

        // act
        var sut = new LoginRequest
        {
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedBody, sut.Body);
    }
}
