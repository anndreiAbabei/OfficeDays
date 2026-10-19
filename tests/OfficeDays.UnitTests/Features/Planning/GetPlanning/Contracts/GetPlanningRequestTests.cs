using OfficeDays.Features.Planning.GetPlanning.Contracts;

namespace OfficeDays.UnitTests.Features.Planning.GetPlanning.Contracts;

public sealed class GetPlanningRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedYear = 21;
        var expectedMonth = 22;

        // act
        var sut = new GetPlanningRequest
        {
            Year = expectedYear,
            Month = expectedMonth
        };

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedMonth, sut.Month);
    }
}
