using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.UnitTests.Features.Status.GetStatus.Contracts;

public sealed class GetStatusResponseTests
{
    private readonly GetStatusSummary _summary = new GetStatusSummary(new DateOnly(2026, 10, 30), 31, 22, 1, 1,
        OfficeDays.Domain.AttendanceRequirementMode.Percentage, 50, 11, 3, 1, 2, [], []);

    [Fact]
    public void Should_SetProperties_WhenConstructingResponse()
    {
        // arrange
        var expectedPeriod = "Period value";
        var expectedEligibleWorkingDays = 22;
        var expectedMaximumWfhDays = 23;
        var expectedRequiredOfficeDays = 24;
        var expectedOfficeDays = 25;
        var expectedRemainingOfficeDays = 26;
        var expectedProgressPercentage = 48.5m;
        var expectedUpcomingPlannedDays = 28;

        // act
        var sut = new GetStatusResponse(expectedPeriod, expectedEligibleWorkingDays, expectedMaximumWfhDays, expectedRequiredOfficeDays, expectedOfficeDays, expectedRemainingOfficeDays, expectedProgressPercentage, _summary, expectedUpcomingPlannedDays);

        // assert
        Assert.Equal(expectedPeriod, sut.Period);
        Assert.Equal(expectedEligibleWorkingDays, sut.EligibleWorkingDays);
        Assert.Equal(expectedMaximumWfhDays, sut.MaximumWfhDays);
        Assert.Equal(expectedRequiredOfficeDays, sut.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, sut.OfficeDays);
        Assert.Equal(expectedRemainingOfficeDays, sut.RemainingOfficeDays);
        Assert.Equal(expectedProgressPercentage, sut.ProgressPercentage);
        Assert.Equal(expectedUpcomingPlannedDays, sut.UpcomingPlannedDays);
        Assert.Same(_summary, sut.Summary);
    }

    [Fact]
    public void Should_DefaultUpcomingPlannedDaysToZero_WhenOmitted()
    {
        // arrange
        var expectedUpcomingPlannedDays = 0;

        // act
        var sut = new GetStatusResponse("2026-10", 20, 10, 10, 7, 3, 70m, _summary);

        // assert
        Assert.Equal(expectedUpcomingPlannedDays, sut.UpcomingPlannedDays);
    }
}
