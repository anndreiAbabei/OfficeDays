using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed class ReplaceBankHolidaysItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedCountryCode = "RO";
        var expectedDate = new DateOnly(2026, 10, 2);
        var expectedName = "Name value";

        // act
        var sut = new ReplaceBankHolidaysItem(expectedCountryCode, expectedDate, expectedName);

        // assert
        Assert.Equal(expectedCountryCode, sut.CountryCode);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedName, sut.Name);
    }
}
