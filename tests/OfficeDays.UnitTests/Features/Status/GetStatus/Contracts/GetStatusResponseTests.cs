using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.UnitTests.Features.Status.GetStatus.Contracts;

public sealed class GetStatusResponseTests
{
    [Fact]
    public void Should_SetProperties()
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
        var sut = new GetStatusResponse(expectedPeriod, expectedEligibleWorkingDays, expectedMaximumWfhDays, expectedRequiredOfficeDays, expectedOfficeDays, expectedRemainingOfficeDays, expectedProgressPercentage, expectedUpcomingPlannedDays);

        // assert
        Assert.Equal(expectedPeriod, sut.Period);
        Assert.Equal(expectedEligibleWorkingDays, sut.EligibleWorkingDays);
        Assert.Equal(expectedMaximumWfhDays, sut.MaximumWfhDays);
        Assert.Equal(expectedRequiredOfficeDays, sut.RequiredOfficeDays);
        Assert.Equal(expectedOfficeDays, sut.OfficeDays);
        Assert.Equal(expectedRemainingOfficeDays, sut.RemainingOfficeDays);
        Assert.Equal(expectedProgressPercentage, sut.ProgressPercentage);
        Assert.Equal(expectedUpcomingPlannedDays, sut.UpcomingPlannedDays);
    }

    [Fact]
    public void Should_DefaultUpcomingPlannedDaysToZero()
    {
        // arrange
        var expectedUpcomingPlannedDays = 0;

        // act
        var sut = new GetStatusResponse("2026-10", 20, 10, 10, 7, 3, 70m);

        // assert
        Assert.Equal(expectedUpcomingPlannedDays, sut.UpcomingPlannedDays);
    }
}
