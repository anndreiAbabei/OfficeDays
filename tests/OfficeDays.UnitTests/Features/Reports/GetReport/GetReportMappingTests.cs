using OfficeDays.Features.Reports.GetReport;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Reports.GetReport;

public sealed class GetReportMappingTests
{
    [Theory]
    [InlineData(ReportMonthState.NotStarted, "notStarted")]
    [InlineData(ReportMonthState.InProgress, "inProgress")]
    [InlineData(ReportMonthState.TargetMet, "targetMet")]
    [InlineData(ReportMonthState.BelowTarget, "belowTarget")]
    public void Should_MapMonthState(ReportMonthState state, string expectedResult)
    {
        // arrange
        var sut = new AttendanceReportMonth(6, 20, 60, 12, 13, 65m, state);

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Month, result.Month);
        Assert.Equal(sut.EligibleWorkingDays, result.EligibleWorkingDays);
        Assert.Equal(sut.RequirementValue, result.RequirementValue);
        Assert.Equal(sut.RequiredOfficeDays, result.RequiredOfficeDays);
        Assert.Equal(sut.OfficeDays, result.OfficeDays);
        Assert.Equal(sut.AttendancePercentage, result.AttendancePercentage);
        Assert.Equal(expectedResult, result.Result);
    }

    [Fact]
    public void Should_PreserveNullMonthTotals()
    {
        // arrange
        var sut = new AttendanceReportMonth(6, 20, 60, 12, null, null, ReportMonthState.NotStarted);

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Null(result.OfficeDays);
        Assert.Null(result.AttendancePercentage);
    }

    [Fact]
    public void Should_RejectUnknownMonthState()
    {
        // arrange
        var sut = new AttendanceReportMonth(6, 20, 60, 12, 13, 65m, (ReportMonthState)99);

        // act
        var result = Assert.Throws<ArgumentOutOfRangeException>(() => sut.ToViewModel());

        // assert
        Assert.Equal("month", result.ParamName);
    }

    [Fact]
    public void Should_MapReportAndSummary()
    {
        // arrange
        AttendanceReportMonth[] months =
        [
            new AttendanceReportMonth(5, 21, 50, 11, 12, 57.14m, ReportMonthState.TargetMet),
            new AttendanceReportMonth(6, 20, 60, 12, null, null, ReportMonthState.NotStarted)
        ];
        var sut = new AttendanceReport(2026, 5, 6, "alice", "Europe/Bucharest", new DateOnly(2025, 1, 1), new DateOnly(2026, 5, 31), new DateTimeOffset(2026, 5, 31, 9, 0, 0, TimeSpan.Zero), months, 12, 57.14m, 1, 1);

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Equal(sut.Year, result.Year);
        Assert.Equal(sut.StartMonth, result.StartMonth);
        Assert.Equal(sut.EndMonth, result.EndMonth);
        Assert.Equal(sut.Username, result.Username);
        Assert.Equal(sut.TimeZoneId, result.TimeZoneId);
        Assert.Equal(sut.RegistrationMonth, result.RegistrationMonth);
        Assert.Equal(sut.Today, result.Today);
        Assert.Equal(sut.GeneratedAt, result.GeneratedAt);
        Assert.Equal(sut.OfficeDays, result.Summary.OfficeDays);
        Assert.Equal(sut.AttendancePercentage, result.Summary.AttendancePercentage);
        Assert.Equal(sut.CompletedMonths, result.Summary.CompletedMonths);
        Assert.Equal(sut.MonthsMeetingTarget, result.Summary.MonthsMeetingTarget);
        Assert.Equal(new[] { 5, 6 }, result.Months.Select(month => month.Month));
        Assert.Equal(new[] { "targetMet", "notStarted" }, result.Months.Select(month => month.Result));
    }

    [Fact]
    public void Should_MapEmptyReport()
    {
        // arrange
        var sut = new AttendanceReport(2026, 1, 12, "alice", "UTC", DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now), DateTimeOffset.UnixEpoch, [], 0, null, 0, 0);

        // act
        var result = sut.ToViewModel();

        // assert
        Assert.Empty(result.Months);
        Assert.Null(result.Summary.AttendancePercentage);
        Assert.Equal(0, result.Summary.OfficeDays);
    }
}
