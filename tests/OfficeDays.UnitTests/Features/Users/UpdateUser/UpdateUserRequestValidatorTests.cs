using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Features.Users.UpdateUser;

namespace OfficeDays.UnitTests.Features.Users.UpdateUser;

public sealed class UpdateUserRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new UpdateUserRequestValidator(new UpdateUserRequestBodyValidator());
        var request = new UpdateUserRequest { Body = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new UpdateUserRequestValidator(new UpdateUserRequestBodyValidator());
        var request = new UpdateUserRequest { Body = null! };

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
        var sut = new UpdateUserRequestValidator(new UpdateUserRequestBodyValidator());
        var request = new UpdateUserRequest { Body = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { Email = "invalid" } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Email");
    }
}
