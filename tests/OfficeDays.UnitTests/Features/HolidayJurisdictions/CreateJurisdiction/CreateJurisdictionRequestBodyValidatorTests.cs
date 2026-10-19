using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionRequestBodyValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenBodyIsValid()
    {
        // arrange
        var request = new CreateJurisdictionRequestBody("RO", "Romania");
        var sut = new CreateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsNull()
    {
        // arrange
        var request = new CreateJurisdictionRequestBody("RO", "Romania") with { Code = null! };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania") with { Code = "" };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania") with { Code = "R" };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania") with { Code = "RO-TOOLONG" };
        var sut = new CreateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "code");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenNameIsNull()
    {
        // arrange
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Name = null!
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Name = ""
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Name = " "
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Name = new string('x', 101)
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

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
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Name = new string('x', 100)
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsLowercaseSubdivision()
    {
        // arrange
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Code = "gb-nir"
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsPaddedCountryCode()
    {
        // arrange
        var request = new CreateJurisdictionRequestBody("RO", "Romania")
        {
            Code = " US "
        };
        var sut = new CreateJurisdictionRequestBodyValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
