using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.UnitTests.Features.Status.GetStatus.Contracts;

public sealed class GetStatusHolidayTests
{
    [Fact]
    public void Should_PreserveHolidayValuesAndEquality_WhenConstructingHoliday()
    {
        // Arrange
        var expectedDate = new DateOnly(2026, 10, 1);
        const string expectedName = "Holiday";
        var expectedHoliday = new GetStatusHoliday(expectedDate, expectedName);

        // Act
        var result = new GetStatusHoliday(expectedDate, expectedName);

        // Assert
        Assert.Equal(expectedDate, result.Date);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedHoliday, result);
    }
}
