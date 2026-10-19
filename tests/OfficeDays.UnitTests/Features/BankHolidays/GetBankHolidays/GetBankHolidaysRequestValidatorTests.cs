using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Features.BankHolidays.GetBankHolidays;

namespace OfficeDays.UnitTests.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequestIsValid()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsNull()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = null! };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "countryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsEmpty()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = "" };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "countryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsTooShort()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = "R" };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "countryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsTooLong()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = "TOOLONG" };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "countryCode");
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsSubdivisionTooLong()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = "RO-TOOLONG" };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "countryCode");
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsLowercaseSubdivision()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = "gb-nir" };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsPaddedCountryCode()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { CountryCode = " US " };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs0()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { Year = 0 };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs10000()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { Year = 10000 };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs1()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { Year = 1 };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs9999()
    {
        // arrange
        var request = new GetBankHolidaysRequest { CountryCode = "RO", Year = 2026 } with { Year = 9999 };
        var sut = new GetBankHolidaysRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
