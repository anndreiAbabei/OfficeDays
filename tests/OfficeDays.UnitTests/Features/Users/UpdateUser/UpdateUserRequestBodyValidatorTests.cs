using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Features.Users.UpdateUser;

namespace OfficeDays.UnitTests.Features.Users.UpdateUser;

public sealed class UpdateUserRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenEmailIsMalformed()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { Email = "invalid" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTeamDayIsSaturday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "saturday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TeamDay");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTeamDayIsSunday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "sunday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TeamDay");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTeamDayIsMondayWithUppercase()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "Monday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TeamDay");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenTeamDayIsEmpty()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TeamDay");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsNull()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { Email = null };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsEmpty()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { Email = "" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenEmailIsWhitespace()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { Email = " " };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenTeamDayIsNull()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = null };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenTeamDayIsTuesday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "tuesday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenTeamDayIsWednesday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "wednesday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenTeamDayIsThursday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "thursday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenTeamDayIsFriday()
    {
        // arrange
        var request = new UpdateUserRequestBody("user@example.com") { TeamDay = "monday" } with { TeamDay = "friday" };
        var sut = new UpdateUserRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
