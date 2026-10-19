using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequestIsValid()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsNull()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = null! };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsEmpty()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = "" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsTooShort()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = "R" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsToolong()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = "TOOLONG" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsTooLong()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = "RO-TOOLONG" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsLowercaseSubdivision()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = "gb-nir" };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsPaddedCountryCode()
    {
        // arrange
        var request = new DeleteJurisdictionRequest { Code = "RO" } with { Code = " US " };
        var sut = new DeleteJurisdictionRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
