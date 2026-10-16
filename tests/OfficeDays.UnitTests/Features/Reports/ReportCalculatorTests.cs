using OfficeDays.Domain;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Reports;

public sealed class ReportCalculatorTests
{
    private static User User(DateTimeOffset? registered = null, string zone = "UTC") => new()
    {
        Id = Guid.NewGuid(), Username = "tester", NormalizedUsername = "TESTER", PasswordHash = "hash",
        TimeZoneId = zone, CreatedAt = registered ?? new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero)
    };
    private static readonly DateTimeOffset Now = new(2026, 10, 16, 9, 0, 0, TimeSpan.Zero);
    private static UserRequiredOfficePercentage Requirement(string date, int percentage) => new()
    {
        StartingFrom = DateOnly.Parse(date), RequiredOfficePercentage = percentage
    };
    private readonly ReportCalculator _calculator = new(new AttendanceCalculator());

    [Fact]
    public void Clips_at_registration_and_distinguishes_completed_current_and_future_months()
    {
        var report = _calculator.Calculate(2026, 1, 12, User(), Now,
            [Requirement("0001-01-01", 0), Requirement("2026-09-01", 50), Requirement("2026-12-01", 60)], [], [], []);
        Assert.Equal(Enumerable.Range(6, 7), report.Months.Select(x => x.Month));
        Assert.Equal(ReportMonthState.TargetMet, report.Months[0].State);
        Assert.Equal(ReportMonthState.BelowTarget, report.Months[3].State);
        Assert.Equal(ReportMonthState.InProgress, report.Months[4].State);
        Assert.Equal(ReportMonthState.NotStarted, report.Months[5].State);
        Assert.Equal(60, report.Months[6].RequiredOfficePercentage);
        Assert.Null(report.Months[6].OfficeDays);
        Assert.Null(report.Months[6].AttendancePercentage);
        Assert.Equal(4, report.CompletedMonths);
        Assert.Equal(3, report.MonthsMeetingTarget);
        Assert.Equal(0, report.OfficeDays);
    }

    [Fact]
    public void Deduplicates_time_off_and_counts_only_eligible_recorded_attendance()
    {
        // June 2026 has 22 weekdays. A holiday and two vacation weekdays leave 19.
        var report = _calculator.Calculate(2026, 6, 6, User(), Now, [Requirement("0001-01-01", 50)],
            [new(2026, 6, 1)], [(new(2026, 6, 1), new(2026, 6, 3)), (new(2026, 6, 2), new(2026, 6, 3))],
            [new(2026, 6, 1), new(2026, 6, 2), new(2026, 6, 4), new(2026, 6, 4), new(2026, 6, 6)]);
        var month = Assert.Single(report.Months);
        Assert.Equal(19, month.EligibleWorkingDays);
        Assert.Equal(10, month.RequiredOfficeDays);
        Assert.Equal(1, month.OfficeDays);
        Assert.Equal(5.3m, month.AttendancePercentage);
    }

    [Fact]
    public void Summary_uses_weighted_eligible_days_and_excludes_future_months()
    {
        var report = _calculator.Calculate(2026, 9, 12, User(), Now, [Requirement("0001-01-01", 100)], [], [],
            [new(2026, 9, 1), new(2026, 9, 2), new(2026, 10, 1), new(2026, 10, 30), new(2026, 11, 2)]);
        Assert.Equal(3, report.OfficeDays); // Future recorded dates are never counted.
        Assert.Equal(Math.Round(3m * 100 / 44, 1), report.AttendancePercentage); // Sep and Oct: 22 weekdays each.
        Assert.Equal(1, report.CompletedMonths);
        Assert.Equal(0, report.MonthsMeetingTarget);
    }

    [Fact]
    public void Empty_ranges_and_months_without_eligible_days_have_no_percentage()
    {
        var empty = _calculator.Calculate(2025, 1, 12, User(), Now, [Requirement("0001-01-01", 50)], [], [], []);
        Assert.Empty(empty.Months);
        Assert.Null(empty.AttendancePercentage);
        var vacation = _calculator.Calculate(2026, 6, 6, User(), Now, [Requirement("0001-01-01", 50)], [],
            [(new(2026, 6, 1), new(2026, 6, 30))], []);
        Assert.Equal(0, vacation.Months[0].RequiredOfficeDays);
        Assert.Null(vacation.Months[0].AttendancePercentage);
        Assert.Equal(ReportMonthState.TargetMet, vacation.Months[0].State);
    }

    [Fact]
    public void Registration_and_current_month_use_the_users_timezone_at_month_boundaries()
    {
        var registered = new DateTimeOffset(2026, 5, 31, 22, 30, 0, TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 9, 30, 22, 30, 0, TimeSpan.Zero);
        var report = _calculator.Calculate(2026, 1, 12, User(registered, "Europe/Bucharest"), now,
            [Requirement("0001-01-01", 50)], [], [], []);
        Assert.Equal(new DateOnly(2026, 6, 1), report.RegistrationMonth);
        Assert.Equal(new DateOnly(2026, 10, 1), report.Today);
        Assert.Equal(ReportMonthState.InProgress, report.Months.Single(x => x.Month == 10).State);
    }

    [Fact]
    public void Leap_year_and_year_boundaries_preserve_full_month_targets()
    {
        var report = _calculator.Calculate(2024, 2, 2, User(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero)), Now,
            [Requirement("0001-01-01", 50)], [], [], []);
        Assert.Equal(21, report.Months[0].EligibleWorkingDays);
        Assert.Equal(11, report.Months[0].RequiredOfficeDays);
        var future = _calculator.Calculate(2027, 1, 12, User(), Now, [Requirement("0001-01-01", 50)], [], [], []);
        Assert.All(future.Months, x => Assert.Equal(ReportMonthState.NotStarted, x.State));
        Assert.Equal(0, future.CompletedMonths);
        Assert.Null(future.AttendancePercentage);
    }
}
