using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed class ReplaceBankHolidaysRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCountryCode = "RO";
        var expectedYear = 22;
        var expectedBody = new List<BankHolidayItem> { new BankHolidayItem(DateOnly.FromDateTime(DateTime.Now), "Name value") };

        // act
        var sut = new ReplaceBankHolidaysRequest
        {
            CountryCode = expectedCountryCode,
            Year = expectedYear,
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedBody, sut.Body);
    }
}
