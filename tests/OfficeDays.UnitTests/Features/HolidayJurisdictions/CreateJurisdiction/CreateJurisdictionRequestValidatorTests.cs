using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var sut = new CreateJurisdictionRequestValidator(new CreateJurisdictionRequestBodyValidator());
        var request = new CreateJurisdictionRequest { Body = new CreateJurisdictionRequestBody("RO", "Romania") };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenBodyIsNull()
    {
        // arrange
        var sut = new CreateJurisdictionRequestValidator(new CreateJurisdictionRequestBodyValidator());
        var request = new CreateJurisdictionRequest { Body = null! };

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
        var sut = new CreateJurisdictionRequestValidator(new CreateJurisdictionRequestBodyValidator());
        var request = new CreateJurisdictionRequest { Body = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Code = null!
        } };

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body.code");
    }
}
