using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Features.Authentication.Login;

namespace OfficeDays.UnitTests.Features.Authentication.Login;

public sealed class LoginRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new LoginRequestBody("user", "password");
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsNull()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Username = null! };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsEmpty()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Username = "" };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenUsernameIsWhitespace()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Username = " " };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Username");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsNull()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Password = null! };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsEmpty()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Password = "" };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenPasswordIsWhitespace()
    {
        // arrange
        var request = new LoginRequestBody("user", "password") with { Password = " " };
        var sut = new LoginRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }
}
