using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;
using OfficeDays.Features.BankHolidays.ReplaceBankHolidays;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays;

public sealed class ReplaceBankHolidaysRequestValidatorTests
{
    [Fact]
    public void Should_ReturnValidationSuccess_WhenRequestIsValid()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationFailure_WhenCodeIsNull()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = null! };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

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
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = "" };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

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
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = "R" };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

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
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = "TOOLONG" };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

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
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = "RO-TOOLONG" };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

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
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = "gb-nir" };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ReturnValidationSuccess_WhenCodeIsPaddedCountryCode()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { CountryCode = " US " };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs0()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Year = 0, Body = [] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs10000()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Year = 10000, Body = [] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs1()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Year = 1, Body = [] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateYear_WhenYearIs9999()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Year = 9999, Body = [] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyNull()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = null! };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body");
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyEmpty()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyContainsNull()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [null!] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body[0]");
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyHasDuplicateDates()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "First"), new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "Second")] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "date");
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyHasDifferentYear()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [new BankHolidayItem(new DateOnly(2025, 1, 1), "Holiday")] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "date");
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyHasInvalidName()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "")] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Body[0].Name");
    }

    [Fact]
    public void Should_ValidateBody_WhenBodyHasDistinctDates()
    {
        // arrange
        var request = new ReplaceBankHolidaysRequest { CountryCode = "RO", Year = 2026, Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "New Year")] } with { Body = [new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "First"), new BankHolidayItem(new DateOnly(2026, 1, 2), "Second")] };
        var sut = new ReplaceBankHolidaysRequestValidator(new BankHolidayItemValidator());

        // act
        var result = sut.Validate(request);

        // assert
        Assert.True(result.IsValid);
    }
}
