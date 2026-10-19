using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new UpdateJurisdictionRequestBody("Romania");
        var sut = new UpdateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsNull()
    {
        // arrange
        var request = new UpdateJurisdictionRequestBody("Romania") with { Name = null! };
        var sut = new UpdateJurisdictionRequestBodyValidator();

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
        var request = new UpdateJurisdictionRequestBody("Romania") with { Name = "" };
        var sut = new UpdateJurisdictionRequestBodyValidator();

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
        var request = new UpdateJurisdictionRequestBody("Romania") with { Name = " " };
        var sut = new UpdateJurisdictionRequestBodyValidator();

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
        var request = new UpdateJurisdictionRequestBody("Romania") with { Name = new string('x', 101) };
        var sut = new UpdateJurisdictionRequestBodyValidator();

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
        var request = new UpdateJurisdictionRequestBody("Romania") with { Name = new string('x', 100) };
        var sut = new UpdateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
