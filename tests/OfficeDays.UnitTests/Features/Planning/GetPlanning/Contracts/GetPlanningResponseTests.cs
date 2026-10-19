using OfficeDays.Features.Planning.GetPlanning.Contracts;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Planning.GetPlanning.Contracts;

public sealed class GetPlanningResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedToday = DateOnly.FromDateTime(DateTime.Now);
        var expectedUpcomingPlannedDays = 22;
        var expectedDays = new List<PlanningDay> { new PlanningDay(DateOnly.FromDateTime(DateTime.Now), true, true, false, false, true, "planned", [], false) };

        // act
        var sut = new GetPlanningResponse(expectedToday, expectedUpcomingPlannedDays, expectedDays);

        // assert
        Assert.Equal(expectedToday, sut.Today);
        Assert.Equal(expectedUpcomingPlannedDays, sut.UpcomingPlannedDays);
        Assert.Equal(expectedDays, sut.Days);
    }
}
