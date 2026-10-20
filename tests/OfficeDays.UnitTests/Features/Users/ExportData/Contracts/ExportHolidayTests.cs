using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportHolidayTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        const int expectedHolidayJurisdictionId = 42;
        var expectedDate = new DateOnly(2026, 10, 19);
        const string expectedName = "Holiday";

        // act
        var sut = new ExportHoliday(expectedId, expectedHolidayJurisdictionId, expectedDate, expectedName);

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedHolidayJurisdictionId, sut.HolidayJurisdictionId);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedName, sut.Name);
    }
}
