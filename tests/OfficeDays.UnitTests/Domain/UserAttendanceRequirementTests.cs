using OfficeDays.Domain;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Domain;

public sealed class UserAttendanceRequirementTests
{
    [Theory]
    [InlineData(AttendanceRequirementMode.Percentage, 0, 0)]
    [InlineData(AttendanceRequirementMode.Percentage, 37, 37)]
    [InlineData(AttendanceRequirementMode.Percentage, 100, 100)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 0, 0)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 3, 60)]
    [InlineData(AttendanceRequirementMode.DaysPerWeek, 5, 100)]
    public void Should_DerivePercentageWithoutLosingOriginalValue_WhenReadingRequirement(AttendanceRequirementMode mode, int value, int expectedPercentage)
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startingFrom = new DateOnly(2026, 11, 1);
        var createdAt = new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
        var requirement = new UserAttendanceRequirement
        {
            Id = id, UserId = userId, StartingFrom = startingFrom, CreatedAt = createdAt, Mode = mode, Value = value
        };

        // Act
        var percentage = requirement.RequiredOfficePercentage;

        // Assert
        Assert.Equal(expectedPercentage, percentage);
        Assert.Equal(mode, requirement.Mode);
        Assert.Equal(value, requirement.Value);
        Assert.Equal(id, requirement.Id);
        Assert.Equal(userId, requirement.UserId);
        Assert.Equal(startingFrom, requirement.StartingFrom);
        Assert.Equal(createdAt, requirement.CreatedAt);
    }

    [Theory]
    [InlineData(2026, 2, 0, 12)] // 20 eligible days.
    [InlineData(2026, 4, 0, 14)] // 22 eligible days, round once.
    [InlineData(2026, 2, 2, 11)] // 18 eligible days after time off.
    [InlineData(2028, 2, 0, 13)] // Leap-year February.
    [InlineData(2026, 2, 28, 0)] // Entire month off.
    public void Should_CalculateMonthlyQuota_WhenUsingWeeklyDays(int year, int month, int vacationDays, int expectedDays)
    {
        // Arrange
        var requirement = new UserAttendanceRequirement { Mode = AttendanceRequirementMode.DaysPerWeek, Value = 3 };
        var calculator = new AttendanceCalculator();
        var period = CalculationPeriod.ForMonth(year, month);
        var firstMonday = new DateOnly(year, month, 2);
        (DateOnly From, DateOnly To)[] vacations = vacationDays == 0 ? []
            : vacationDays == 28 ? [(period.Start, period.End)]
            : [(firstMonday, firstMonday.AddDays(vacationDays - 1))];

        // Act
        var result = calculator.Calculate(period, [], vacations, [], requirement.RequiredOfficePercentage);

        // Assert
        Assert.Equal(expectedDays, result.RequiredOfficeDays);
    }

    [Fact]
    public void Should_MeetQuota_WhenAttendanceIsConcentratedInTwoWeeksAndTwoLaterDays()
    {
        // Arrange
        var calculator = new AttendanceCalculator();
        var requirement = new UserAttendanceRequirement { Mode = AttendanceRequirementMode.DaysPerWeek, Value = 3 };
        var period = CalculationPeriod.ForMonth(2026, 2);
        var attendance = new[] { 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 26, 27 }.Select(day => new DateOnly(2026, 2, day));
        const int expectedDays = 12;
        const int expectedRemainingDays = 0;

        // Act
        var result = calculator.Calculate(period, [], [], attendance, requirement.RequiredOfficePercentage);

        // Assert
        Assert.Equal(expectedDays, result.RequiredOfficeDays);
        Assert.Equal(expectedDays, result.OfficeDays);
        Assert.Equal(expectedRemainingDays, result.RemainingOfficeDays);
    }

    [Fact]
    public void Should_ExcludeHolidayAndVacationOverlapOnce_WhenCalculatingWeeklyQuota()
    {
        // Arrange
        var calculator = new AttendanceCalculator();
        var period = CalculationPeriod.ForMonth(2026, 2);
        var holiday = new DateOnly(2026, 2, 2);
        var requirement = new UserAttendanceRequirement { Mode = AttendanceRequirementMode.DaysPerWeek, Value = 3 };
        const int expectedEligibleDays = 18;
        const int expectedRequiredDays = 11;
        const int expectedOfficeDays = 0;

        // Act
        var result = calculator.Calculate(period, [holiday], [(holiday, holiday.AddDays(1))], [holiday], requirement.RequiredOfficePercentage);

        // Assert
        Assert.Equal(expectedEligibleDays, result.EligibleWorkingDays);
        Assert.Equal(expectedRequiredDays, result.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, result.OfficeDays);
    }
}
