using OfficeDays.Features.Reports.GetReport.Contracts;

namespace OfficeDays.UnitTests.Features.Reports.GetReport.Contracts;

public sealed class GetReportSummaryTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedOfficeDays = 21;
        var expectedAttendancePercentage = 43.5m;
        var expectedCompletedMonths = 23;
        var expectedMonthsMeetingTarget = 24;

        // act
        var sut = new GetReportSummary(expectedOfficeDays, expectedAttendancePercentage, expectedCompletedMonths, expectedMonthsMeetingTarget);

        // assert
        Assert.Equal(expectedOfficeDays, sut.OfficeDays);
        Assert.Equal(expectedAttendancePercentage, sut.AttendancePercentage);
        Assert.Equal(expectedCompletedMonths, sut.CompletedMonths);
        Assert.Equal(expectedMonthsMeetingTarget, sut.MonthsMeetingTarget);
    }
}
