using OfficeDays.Features.Status.GetStatus;
using OfficeDays.Services;
using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusMappingTests
{
    [Fact]
    public void Should_MapEveryField_WhenCreatingStatusResponse()
    {
        // arrange
        var sut = new AttendanceStatus("2026-10", 22, 10, 12, 8, 4, 66.67m);
        var summary = GetStatusSummaryCalculator.Calculate([], new DateOnly(2026, 10, 30), sut,
            new UserAttendanceRequirement { RequirementValue = 50 });
        const int expectedUpcomingPlannedDays = 3;

        // act
        var result = sut.ToViewModel(summary, expectedUpcomingPlannedDays);

        // assert
        Assert.Equal(sut.Period, result.Period);
        Assert.Equal(sut.EligibleWorkingDays, result.EligibleWorkingDays);
        Assert.Equal(sut.MaximumWfhDays, result.MaximumWfhDays);
        Assert.Equal(sut.RequiredOfficeDays, result.RequiredOfficeDays);
        Assert.Equal(sut.OfficeDays, result.OfficeDays);
        Assert.Equal(sut.RemainingOfficeDays, result.RemainingOfficeDays);
        Assert.Equal(sut.ProgressPercentage, result.ProgressPercentage);
        Assert.Equal(expectedUpcomingPlannedDays, result.UpcomingPlannedDays);
        Assert.Same(summary, result.Summary);
    }
}
