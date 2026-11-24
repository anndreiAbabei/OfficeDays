using OfficeDays.Features.Authentication.Login.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.Login.Contracts;

public sealed class LoginRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedUsername = "Username value";
        const string expectedPassword = "a-long-password";
        const bool expectedRememberMe = true;

        // act
        var sut = new LoginRequestBody(expectedUsername, expectedPassword, expectedRememberMe);

        // assert
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedPassword, sut.Password);
        Assert.Equal(expectedRememberMe, sut.RememberMe);
    }

    [Fact]
    public void Should_HaveRememberMe_FalseByDefault()
    {
        // arrange
        // act
        var sut = new LoginRequestBody("", "");

        // assert
        Assert.False(sut.RememberMe);
    }
}
