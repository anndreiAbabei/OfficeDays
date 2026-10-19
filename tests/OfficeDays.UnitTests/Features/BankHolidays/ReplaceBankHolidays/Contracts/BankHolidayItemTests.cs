using OfficeDays.Features.BankHolidays.ReplaceBankHolidays.Contracts;

namespace OfficeDays.UnitTests.Features.BankHolidays.ReplaceBankHolidays.Contracts;

public sealed class BankHolidayItemTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        const string expectedName = "Name value";

        // act
        var sut = new BankHolidayItem(expectedDate, expectedName);

        // assert
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedName, sut.Name);
    }
}
