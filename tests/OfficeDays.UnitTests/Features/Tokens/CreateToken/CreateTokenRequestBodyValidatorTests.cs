using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Features.Tokens.CreateToken;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenRequestBodyValidatorTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("ios-shortcuts", true)]
    [InlineData("", false)]
    [InlineData("unknown", false)]
    public void Should_ValidatePurpose_WhenCreatingToken(string? purpose, bool expectedValid)
    {
        // Arrange
        var request = new CreateTokenRequestBody("Work", purpose);
        var validator = new CreateTokenRequestBodyValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

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
        var request = new CreateTokenRequestBody(Name: null!);
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
        var request = new CreateTokenRequestBody(Name: "");
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
        var request = new CreateTokenRequestBody(Name: " ");
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
        var request = new CreateTokenRequestBody(Name: new string('x', 101));
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
        var request = new CreateTokenRequestBody(Name: new string('x', 100));
        var sut = new CreateTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
