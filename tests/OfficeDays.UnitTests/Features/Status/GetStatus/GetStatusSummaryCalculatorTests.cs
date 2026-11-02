using OfficeDays.Domain;
using OfficeDays.Features.Status.GetStatus;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusSummaryCalculatorTests
{
    [Theory]
    [InlineData(2026, 10, 31, 22)]
    [InlineData(2028, 2, 29, 21)]
    [InlineData(2027, 2, 28, 20)]
    [InlineData(2026, 12, 31, 23)]
    public void Should_CountCalendarDaysAndWeekdays_WhenMonthLengthVaries(int year, int month, int expectedDays, int expectedWeekdays)
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(year, month);
        var expectedToday = period.Start;
        var expectedRequired = (int)Math.Ceiling(expectedWeekdays / 2m);

        // Act
        var result = Calculate(period, expectedToday);

        // Assert
        Assert.Equal(expectedToday, result.Today);
        Assert.Equal(expectedDays, result.TotalDays);
        Assert.Equal(expectedWeekdays, result.WorkingDays);
        Assert.Equal(expectedRequired, result.RequiredOfficeDaysWithoutAbsences);
        Assert.Empty(result.BankHolidays);
        Assert.Empty(result.VacationDates);
    }

    [Fact]
    public void Should_DeductEachWeekdayOnceAndListMonthDates_WhenAbsencesOverlapAndCrossMonthBoundaries()
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        DateOnly[] holidays = [new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 3)];
        (DateOnly, DateOnly)[] vacations = [(new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 5)),
                                           (new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 5))];
        const int expectedHolidayWeekdays = 1;
        const int expectedVacationWeekdays = 2;
        const string expectedHolidayName = "Holiday";
        var expectedHolidayDates = holidays.Skip(1).ToArray();
        var expectedVacationDates = Enumerable.Range(1, 5).Select(day => new DateOnly(2026, 10, day)).ToArray();

        // Act
        var result = Calculate(period, period.Start, holidays: holidays, vacations: vacations);

        // Assert
        Assert.Equal(expectedHolidayWeekdays, result.BankHolidayWorkingDays);
        Assert.Equal(expectedVacationWeekdays, result.VacationWorkingDays);
        Assert.Equal(expectedHolidayDates, result.BankHolidays.Select(holiday => holiday.Date));
        Assert.All(result.BankHolidays, holiday => Assert.Equal(expectedHolidayName, holiday.Name));
        Assert.Equal(expectedVacationDates, result.VacationDates);
    }

    [Theory]
    [InlineData(0, AttendanceRequirementMode.Percentage, 0)]
    [InlineData(50, AttendanceRequirementMode.Percentage, 11)]
    [InlineData(100, AttendanceRequirementMode.Percentage, 22)]
    [InlineData(0, AttendanceRequirementMode.DaysPerWeek, 0)]
    [InlineData(3, AttendanceRequirementMode.DaysPerWeek, 14)]
    [InlineData(5, AttendanceRequirementMode.DaysPerWeek, 22)]
    public void Should_PreserveRequirementAndRoundMonthlyBaseline_WhenRequirementModeVaries(int value, AttendanceRequirementMode mode, int expectedRequired)
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);

        // Act
        var result = Calculate(period, period.Start, value, mode);

        // Assert
        Assert.Equal(mode, result.RequirementMode);
        Assert.Equal(value, result.RequirementValue);
        Assert.Equal(expectedRequired, result.RequiredOfficeDaysWithoutAbsences);
    }

    [Theory]
    [InlineData(29, false, 2, 1)]
    [InlineData(30, false, 1, 2)]
    [InlineData(30, true, 0, 2)]
    [InlineData(31, false, 0, 3)]
    public void Should_IncludeOnlyAvailableUnattendedDates_WhenMonthIsAlmostOver(int day, bool attendToday, int expectedAvailable, int expectedOutstanding)
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        var today = new DateOnly(2026, 10, day);
        var attendance = new[] { 1, 2, 5, 6, 7, 8, 9, 12 }.Select(number => new DateOnly(2026, 10, number)).ToList();
        if (attendToday)
            attendance.Add(today);

        // Act
        var result = Calculate(period, today, attendance: attendance.ToArray());

        // Assert
        Assert.Equal(expectedAvailable, result.RemainingEligibleDays);
        Assert.Equal(expectedOutstanding, result.OutstandingOfficeDays);
    }

    [Fact]
    public void Should_ExcludeHolidaysVacationsAndWeekendsFromCapacity_WhenCountingRemainingDays()
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        var today = new DateOnly(2026, 10, 28);
        const int expectedAvailable = 1;
        DateOnly[] holidays = [new DateOnly(2026, 10, 29)];
        (DateOnly, DateOnly)[] vacations = [(new DateOnly(2026, 10, 30), new DateOnly(2026, 10, 31))];

        // Act
        var result = Calculate(period, today, holidays: holidays, vacations: vacations);

        // Assert
        Assert.Equal(expectedAvailable, result.RemainingEligibleDays);
    }

    [Theory]
    [InlineData(2026, 9, 30, 22, 0)]
    [InlineData(2026, 11, 1, 0, 11)]
    [InlineData(2027, 1, 1, 0, 11)]
    public void Should_HandleFutureAndPastMonths_WhenTodayIsOutsideSelectedMonth(int year, int month, int day, int expectedAvailable, int expectedOutstanding)
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        var today = new DateOnly(year, month, day);

        // Act
        var result = Calculate(period, today);

        // Assert
        Assert.Equal(expectedAvailable, result.RemainingEligibleDays);
        Assert.Equal(expectedOutstanding, result.OutstandingOfficeDays);
    }

    [Fact]
    public void Should_KeepPlansSeparateFromAttendance_WhenPastAttendedAndIneligiblePlansExist()
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        DateOnly[] plans = [new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 2), new DateOnly(2026, 10, 3), new DateOnly(2026, 10, 30)];
        const int expectedPlanned = 3;
        const int expectedOutstanding = 9;

        // Act
        var result = Calculate(period, new DateOnly(2026, 10, 30), plans: plans, attendance: [plans[0]]);

        // Assert
        Assert.Equal(expectedPlanned, result.PlannedDays);
        Assert.Equal(expectedOutstanding, result.OutstandingOfficeDays);
    }

    [Fact]
    public void Should_HaveNoOutstandingDays_WhenWholeMonthIsVacation()
    {
        // Arrange
        var period = CalculationPeriod.ForMonth(2026, 10);
        const int expectedZero = 0;

        // Act
        var result = Calculate(period, period.End, vacations: [(period.Start, period.End)]);

        // Assert
        Assert.Equal(expectedZero, result.RemainingEligibleDays);
        Assert.Equal(expectedZero, result.OutstandingOfficeDays);
    }

    private static GetStatusSummary Calculate(CalculationPeriod period, DateOnly today,
        int value = 50, AttendanceRequirementMode mode = AttendanceRequirementMode.Percentage,
        DateOnly[]? holidays = null, (DateOnly From, DateOnly To)[]? vacations = null,
        DateOnly[]? attendance = null, DateOnly[]? plans = null)
    {
        var requirement = new UserAttendanceRequirement { RequirementMode = mode, RequirementValue = value };
        var calculator = new AttendanceCalculator();
        var status = calculator.Calculate(period, holidays ?? [], vacations ?? [], attendance ?? [], requirement.EffectivePercentage);
        var days = PlanningCalculator.Calculate(period, today, plans ?? [], attendance ?? [],
            (holidays ?? []).Select(date => (date, "Holiday")), vacations ?? []);
        return GetStatusSummaryCalculator.Calculate(days, today, status, requirement);
    }
}
