using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportHolidayCalendarTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const int expectedId = 42;
        const string expectedCode = "RO";
        const string expectedName = "Romania";
        var expectedHolidays = new[] { new ExportHoliday(Guid.NewGuid(), 42, new DateOnly(2026, 10, 19), "Holiday") };

        // act
        var sut = new ExportHolidayCalendar(expectedId, expectedCode, expectedName, expectedHolidays);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedCode, sut.Code);
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedHolidays, sut.Holidays);
    }
}
