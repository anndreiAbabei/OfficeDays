using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Features.Tokens.CreateToken;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new CreateTokenRequestValidator(new CreateTokenRequestBodyValidator());
        var request = new CreateTokenRequest { Body = new CreateTokenRequestBody("Desktop") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new CreateTokenRequestValidator(new CreateTokenRequestBodyValidator());
        var request = new CreateTokenRequest { Body = null! };

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
        var sut = new CreateTokenRequestValidator(new CreateTokenRequestBodyValidator());
        var request = new CreateTokenRequest { Body = new CreateTokenRequestBody("Desktop") with { Name = null! } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Name");
    }
}
