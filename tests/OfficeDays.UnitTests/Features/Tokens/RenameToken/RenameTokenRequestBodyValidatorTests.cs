using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Features.Tokens.RenameToken;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken;

public sealed class RenameTokenRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new RenameTokenRequestBody("Desktop");
        var sut = new RenameTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsNull()
    {
        // arrange
        var request = new RenameTokenRequestBody("Desktop") with { Name = null! };
        var sut = new RenameTokenRequestBodyValidator();

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
        var request = new RenameTokenRequestBody("Desktop") with { Name = "" };
        var sut = new RenameTokenRequestBodyValidator();

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
        var request = new RenameTokenRequestBody("Desktop") with { Name = " " };
        var sut = new RenameTokenRequestBodyValidator();

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
        var request = new RenameTokenRequestBody("Desktop") with { Name = new string('x', 101) };
        var sut = new RenameTokenRequestBodyValidator();

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
        var request = new RenameTokenRequestBody("Desktop") with { Name = new string('x', 100) };
        var sut = new RenameTokenRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
