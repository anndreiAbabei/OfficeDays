using OfficeDays.Domain;
using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.UnitTests.Features.Status.GetStatus.Contracts;

public sealed class GetStatusSummaryTests
{
    [Fact]
    public void Should_PreserveEveryValue_WhenConstructingSummary()
    {
        // Arrange
        var expectedToday = new DateOnly(2026, 10, 30);
        const int expectedTotalDays = 31;
        const int expectedWorkingDays = 22;
        const int expectedHolidayDays = 1;
        const int expectedVacationDays = 2;
        const AttendanceRequirementMode expectedMode = AttendanceRequirementMode.DaysPerWeek;
        const int expectedValue = 3;
        const int expectedBaseline = 14;
        const int expectedPlanned = 5;
        const int expectedAvailable = 1;
        const int expectedOutstanding = 4;
        GetStatusHoliday[] expectedHolidays = [new GetStatusHoliday(new DateOnly(2026, 10, 1), "Holiday")];
        DateOnly[] expectedVacations = [new DateOnly(2026, 10, 2)];

        // Act
        var result = new GetStatusSummary(expectedToday, expectedTotalDays, expectedWorkingDays, expectedHolidayDays,
            expectedVacationDays, expectedMode, expectedValue, expectedBaseline, expectedPlanned, expectedAvailable,
            expectedOutstanding, expectedHolidays, expectedVacations);

        // Assert
        Assert.Equal(expectedToday, result.Today);
        Assert.Equal(expectedTotalDays, result.TotalDays);
        Assert.Equal(expectedWorkingDays, result.WorkingDays);
        Assert.Equal(expectedHolidayDays, result.BankHolidayWorkingDays);
        Assert.Equal(expectedVacationDays, result.VacationWorkingDays);
        Assert.Equal(expectedMode, result.RequirementMode);
        Assert.Equal(expectedValue, result.RequirementValue);
        Assert.Equal(expectedBaseline, result.RequiredOfficeDaysWithoutAbsences);
        Assert.Equal(expectedPlanned, result.PlannedDays);
        Assert.Equal(expectedAvailable, result.RemainingEligibleDays);
        Assert.Equal(expectedOutstanding, result.OutstandingOfficeDays);
        Assert.Same(expectedHolidays, result.BankHolidays);
        Assert.Same(expectedVacations, result.VacationDates);
        Assert.Equal(result, result with { });
    }

}
