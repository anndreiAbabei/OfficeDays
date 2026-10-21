using OfficeDays.Domain;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Reports;

public sealed class ReportCalculatorTests
{
    private static User User(DateTimeOffset? registered = null, string zone = "UTC") => new User
    {
        Id = Guid.NewGuid(), Username = "tester", NormalizedUsername = "TESTER", PasswordHash = "hash",
        TimeZoneId = zone, CreatedAt = registered ?? new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero)
    };
    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 10, 16, 9, 0, 0, TimeSpan.Zero);
    private static UserAttendanceRequirement Requirement(string date, int percentage) => new UserAttendanceRequirement
    {
        StartingFrom = DateOnly.Parse(date), Value = percentage
    };
    private readonly ReportCalculator _calculator = new ReportCalculator(new AttendanceCalculator());

    [Fact]
    public void Should_ClipAndClassifyMonths_WhenRangeSpansRegistrationAndFuture()
    {
        // arrange
        var expectedMonths = Enumerable.Range(6, 7);
        var expectedTargetMetState = ReportMonthState.TargetMet;
        var expectedBelowTargetState = ReportMonthState.BelowTarget;
        var expectedCurrentState = ReportMonthState.InProgress;
        var expectedFutureState = ReportMonthState.NotStarted;
        var expectedRequiredOfficePercentage = 60;
        var expectedCompletedMonths = 4;
        var expectedMonthsMeetingTarget = 3;
        var expectedOfficeDays = 0;

        // act
        var report = _calculator.Calculate(2026, 1, 12, User(), _now,
            [Requirement("0001-01-01", 0), Requirement("2026-09-01", 50), Requirement("2026-12-01", 60)], [], [], []);

        // assert
        Assert.Equal(expectedMonths, report.Months.Select(x => x.Month));
        Assert.Equal(expectedTargetMetState, report.Months[0].State);
        Assert.Equal(expectedBelowTargetState, report.Months[3].State);
        Assert.Equal(expectedCurrentState, report.Months[4].State);
        Assert.Equal(expectedFutureState, report.Months[5].State);
        Assert.Equal(expectedRequiredOfficePercentage, report.Months[6].RequiredOfficePercentage);
        Assert.Null(report.Months[6].OfficeDays);
        Assert.Null(report.Months[6].AttendancePercentage);
        Assert.Equal(expectedCompletedMonths, report.CompletedMonths);
        Assert.Equal(expectedMonthsMeetingTarget, report.MonthsMeetingTarget);
        Assert.Equal(expectedOfficeDays, report.OfficeDays);
    }

    [Fact]
    public void Should_CountOnlyEligibleAttendance_WhenTimeOffOverlaps()
    {
        // arrange
        var expectedEligibleWorkingDays = 19;
        var expectedRequiredOfficeDays = 10;
        var expectedOfficeDays = 1;
        var expectedAttendancePercentage = 5.3m;

        // act
        // June 2026 has 22 weekdays. A holiday and two vacation weekdays leave 19.
        var report = _calculator.Calculate(2026, 6, 6, User(), _now, [Requirement("0001-01-01", 50)],
            [new DateOnly(2026, 6, 1)], [(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 3)), (new DateOnly(2026, 6, 2), new DateOnly(2026, 6, 3))],
            [new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 2), new DateOnly(2026, 6, 4), new DateOnly(2026, 6, 4), new DateOnly(2026, 6, 6)]);

        // assert
        var month = Assert.Single(report.Months);
        Assert.Equal(expectedEligibleWorkingDays, month.EligibleWorkingDays);
        Assert.Equal(expectedRequiredOfficeDays, month.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, month.OfficeDays);
        Assert.Equal(expectedAttendancePercentage, month.AttendancePercentage);
    }

    [Fact]
    public void Should_WeightEligibleDays_WhenSummarizingAttendance()
    {
        // arrange
        var expectedOfficeDays = 3;
        var expectedAttendancePercentage = Math.Round(3m * 100 / 44, 1);
        var expectedCompletedMonths = 1;
        var expectedMonthsMeetingTarget = 0;

        // act
        var report = _calculator.Calculate(2026, 9, 12, User(), _now, [Requirement("0001-01-01", 100)], [], [],
            [new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2), new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 30), new DateOnly(2026, 11, 2)]);

        // assert
        Assert.Equal(expectedOfficeDays, report.OfficeDays); // Future recorded dates are never counted.
        Assert.Equal(expectedAttendancePercentage, report.AttendancePercentage); // Sep and Oct: 22 weekdays each.
        Assert.Equal(expectedCompletedMonths, report.CompletedMonths);
        Assert.Equal(expectedMonthsMeetingTarget, report.MonthsMeetingTarget);
    }

    [Fact]
    public void Should_ReturnNoPercentage_WhenRangeIsEmptyOrNoDaysAreEligible()
    {
        // arrange
        var expectedRequiredOfficeDays = 0;
        var expectedTargetMetState = ReportMonthState.TargetMet;

        // act
        var empty = _calculator.Calculate(2025, 1, 12, User(), _now, [Requirement("0001-01-01", 50)], [], [], []);
        var vacation = _calculator.Calculate(2026, 6, 6, User(), _now, [Requirement("0001-01-01", 50)], [],
            [(new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30))], []);

        // assert
        Assert.Empty(empty.Months);
        Assert.Null(empty.AttendancePercentage);
        Assert.Equal(expectedRequiredOfficeDays, vacation.Months[0].RequiredOfficeDays);
        Assert.Null(vacation.Months[0].AttendancePercentage);
        Assert.Equal(expectedTargetMetState, vacation.Months[0].State);
    }

    [Fact]
    public void Should_UseUserTimezone_WhenDatesCrossMonthBoundaries()
    {
        // arrange
        var expectedRegistrationMonth = new DateOnly(2026, 6, 1);
        var expectedToday = new DateOnly(2026, 10, 1);
        var expectedState = ReportMonthState.InProgress;

        var registered = new DateTimeOffset(2026, 5, 31, 22, 30, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 9, 30, 22, 30, 0, TimeSpan.Zero);
        // act
        var report = _calculator.Calculate(2026, 1, 12, User(registered, "Europe/Bucharest"), now,
            [Requirement("0001-01-01", 50)], [], [], []);

        // assert
        Assert.Equal(expectedRegistrationMonth, report.RegistrationMonth);
        Assert.Equal(expectedToday, report.Today);
        Assert.Equal(expectedState, report.Months.Single(x => x.Month == 10).State);
    }

    [Fact]
    public void Should_PreserveFullMonthTargets_WhenRangeCrossesLeapOrYearBoundaries()
    {
        // arrange
        var expectedEligibleWorkingDays = 21;
        var expectedRequiredOfficeDays = 11;
        var expectedCompletedMonths = 0;
        var expectedFutureState = ReportMonthState.NotStarted;

        // act
        var report = _calculator.Calculate(2024, 2, 2, User(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)), _now,
            [Requirement("0001-01-01", 50)], [], [], []);
        var future = _calculator.Calculate(2027, 1, 12, User(), _now, [Requirement("0001-01-01", 50)], [], [], []);

        // assert
        Assert.Equal(expectedEligibleWorkingDays, report.Months[0].EligibleWorkingDays);
        Assert.Equal(expectedRequiredOfficeDays, report.Months[0].RequiredOfficeDays);
        Assert.All(future.Months, x => Assert.Equal(expectedFutureState, x.State));
        Assert.Equal(expectedCompletedMonths, future.CompletedMonths);
        Assert.Null(future.AttendancePercentage);
    }
}
