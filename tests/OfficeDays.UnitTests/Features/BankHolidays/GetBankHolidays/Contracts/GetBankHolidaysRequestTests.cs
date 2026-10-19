using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.GetBankHolidays.Contracts;

public sealed class GetBankHolidaysRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCountryCode = "RO";
        var expectedYear = 22;

        // act
        var sut = new GetBankHolidaysRequest
        {
            CountryCode = expectedCountryCode,
            Year = expectedYear
        };

        // assert
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedYear, sut.Year);
    }
}
