using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Features.Tokens.RenameToken;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken;

public sealed class RenameTokenRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new RenameTokenRequestValidator(new RenameTokenRequestBodyValidator());
        var request = new RenameTokenRequest { Body = new RenameTokenRequestBody("Desktop") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new RenameTokenRequestValidator(new RenameTokenRequestBodyValidator());
        var request = new RenameTokenRequest { Body = null! };

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
        var sut = new RenameTokenRequestValidator(new RenameTokenRequestBodyValidator());
        var request = new RenameTokenRequest { Body = new RenameTokenRequestBody("Desktop") with { Name = null! } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Name");
    }
}
