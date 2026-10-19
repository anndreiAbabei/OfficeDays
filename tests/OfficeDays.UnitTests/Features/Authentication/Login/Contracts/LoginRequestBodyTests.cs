using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.Login.Contracts;

public sealed class LoginRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedUsername = "Username value";
        var expectedPassword = "a-long-password";

        // act
        var sut = new LoginRequestBody(expectedUsername, expectedPassword);

        // assert
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedPassword, sut.Password);
    }
}
