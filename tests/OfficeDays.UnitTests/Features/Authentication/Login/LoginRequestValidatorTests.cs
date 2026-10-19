using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Features.Authentication.Login;

namespace OfficeDays.UnitTests.Features.Authentication.Login;

public sealed class LoginRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new LoginRequestValidator(new LoginRequestBodyValidator());
        var request = new LoginRequest { Body = new LoginRequestBody("user", "password") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new LoginRequestValidator(new LoginRequestBodyValidator());
        var request = new LoginRequest { Body = null! };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsInvalid()
    {
        // arrange
        var sut = new LoginRequestValidator(new LoginRequestBodyValidator());
        var request = new LoginRequest { Body = new LoginRequestBody("user", "password") with { Username = null! } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Username");
    }
}
