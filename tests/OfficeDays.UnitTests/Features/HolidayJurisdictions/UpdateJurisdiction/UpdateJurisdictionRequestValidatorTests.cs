using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new UpdateJurisdictionRequestValidator(new UpdateJurisdictionRequestBodyValidator());
        var request = new UpdateJurisdictionRequest { Code = "RO", Body = new UpdateJurisdictionRequestBody("Romania") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new UpdateJurisdictionRequestValidator(new UpdateJurisdictionRequestBodyValidator());
        var request = new UpdateJurisdictionRequest { Code = "RO", Body = null! };

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
        var sut = new UpdateJurisdictionRequestValidator(new UpdateJurisdictionRequestBodyValidator());
        var request = new UpdateJurisdictionRequest { Code = "RO", Body = new UpdateJurisdictionRequestBody(Name: null!) };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.Name");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsInvalid()
    {
        // arrange
        var sut = new UpdateJurisdictionRequestValidator(new UpdateJurisdictionRequestBodyValidator());
        var request = new UpdateJurisdictionRequest { Code = "R", Body = new UpdateJurisdictionRequestBody("Romania") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }
}
