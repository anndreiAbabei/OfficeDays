using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Features.Tokens.CreateToken;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop");
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsNull()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop") with { Name = null! };
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsEmpty()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop") with { Name = "" };
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsWhitespace()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop") with { Name = " " };
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsTooLong()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop") with { Name = new string('x', 101) };
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenNameIsMaximumLength()
    {
        // arrange
        var request = new CreateTokenRequestBody("Desktop") with { Name = new string('x', 100) };
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
