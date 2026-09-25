using OfficeDays.Services;

namespace OfficeDays.UnitTests;

public sealed class AttendanceCalculatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(25, 6)]
    [InlineData(50, 11)]
    [InlineData(75, 16)]
    [InlineData(100, 21)]
    public void Custom_percentage_rounds_up_required_days(int percentage, int expected)
    {
        var period = new CalculationPeriod(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 29), "test");
        var result = AttendanceCalculator.Calculate(period, [], [], [], percentage);
        Assert.Equal(expected, result.RequiredOfficeDays);
        Assert.Equal(21 - expected, result.MaximumWfhDays);
        Assert.Equal(expected, result.RemainingOfficeDays);
        Assert.Equal(expected == 0 ? 100m : 0m, result.ProgressPercentage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Invalid_percentage_is_rejected(int percentage)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AttendanceCalculator.Calculate(CalculationPeriod.ForMonth(2026, 9), [], [], [], percentage));
    }

    [Fact]
    public void Odd_eligible_days_enforces_maximum_half_wfh_rule()
    {
        var result = Calculate(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 29)); // 21 weekdays
        Assert.Equal(21, result.EligibleWorkingDays);
        Assert.Equal(10, result.MaximumWfhDays);
        Assert.Equal(11, result.RequiredOfficeDays);
    }

    [Fact]
    public void Even_eligible_days_requires_exactly_half_in_office()
    {
        var result = Calculate(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 28)); // 20 weekdays
        Assert.Equal(20, result.EligibleWorkingDays);
        Assert.Equal(10, result.MaximumWfhDays);
        Assert.Equal(10, result.RequiredOfficeDays);
    }

    [Fact]
    public void Weekends_are_not_eligible()
    {
        var result = Calculate(new DateOnly(2026, 9, 4), new DateOnly(2026, 9, 7)); // Fri through Mon
        Assert.Equal(2, result.EligibleWorkingDays);
    }

    [Fact]
    public void Weekday_bank_holiday_reduces_eligible_days()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11), holidays: [new DateOnly(2026, 9, 9)]);
        Assert.Equal(4, result.EligibleWorkingDays);
    }

    [Fact]
    public void Weekend_bank_holiday_does_not_reduce_days_twice()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13), holidays: [new DateOnly(2026, 9, 12)]);
        Assert.Equal(5, result.EligibleWorkingDays);
    }

    [Fact]
    public void Vacation_weekdays_are_excluded()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11), vacations: [(new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 9))]);
        Assert.Equal(3, result.EligibleWorkingDays);
    }

    [Fact]
    public void Vacation_overlapping_holiday_is_only_excluded_once()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11),
            [new DateOnly(2026, 9, 9)], [(new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 10))]);
        Assert.Equal(2, result.EligibleWorkingDays);
    }

    [Fact]
    public void Weekend_vacation_does_not_reduce_days()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13), vacations: [(new DateOnly(2026, 9, 12), new DateOnly(2026, 9, 13))]);
        Assert.Equal(5, result.EligibleWorkingDays);
    }

    [Fact]
    public void Overlapping_vacations_are_deduplicated()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11), vacations:
            [(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 9)), (new DateOnly(2026, 9, 9), new DateOnly(2026, 9, 10))]);
        Assert.Equal(1, result.EligibleWorkingDays);
    }

    [Fact]
    public void Remaining_days_are_calculated_from_eligible_attendance()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11), office:
            [new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 8)]);
        Assert.Equal(3, result.RequiredOfficeDays);
        Assert.Equal(2, result.OfficeDays);
        Assert.Equal(1, result.RemainingOfficeDays);
    }

    [Fact]
    public void Remaining_days_never_go_below_zero()
    {
        var result = Calculate(new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 11), office:
            [new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 9), new DateOnly(2026, 9, 10)]);
        Assert.Equal(0, result.RemainingOfficeDays);
        Assert.Equal(100m, result.ProgressPercentage);
    }

    [Fact]
    public void Attendance_on_ineligible_days_does_not_count()
    {
        var result = Calculate(new DateOnly(2026, 9, 11), new DateOnly(2026, 9, 14), holidays: [new DateOnly(2026, 9, 14)],
            office: [new DateOnly(2026, 9, 11), new DateOnly(2026, 9, 12), new DateOnly(2026, 9, 14)]);
        Assert.Equal(1, result.OfficeDays);
    }

    [Fact]
    public void Calendar_month_factory_honors_month_boundaries_and_leap_years()
    {
        var february = CalculationPeriod.ForMonth(2028, 2);
        Assert.Equal(new DateOnly(2028, 2, 1), february.Start);
        Assert.Equal(new DateOnly(2028, 2, 29), february.End);
        Assert.Equal("2028-02", february.Label);
    }

    [Fact]
    public void Vacations_crossing_month_boundaries_are_clipped_to_period()
    {
        var result = Calculate(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 4), vacations: [(new DateOnly(2026, 8, 28), new DateOnly(2026, 9, 2))]);
        Assert.Equal(2, result.EligibleWorkingDays);
    }

    private static AttendanceStatus Calculate(DateOnly from, DateOnly to,
        IEnumerable<DateOnly>? holidays = null,
        IEnumerable<(DateOnly From, DateOnly To)>? vacations = null,
        IEnumerable<DateOnly>? office = null) =>
        AttendanceCalculator.Calculate(new CalculationPeriod(from, to, "test"),
            holidays ?? [], vacations ?? [], office ?? []);
}
