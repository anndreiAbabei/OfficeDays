using OfficeDays.Features.Planning.GetPlanning.Contracts;
using OfficeDays.Features.Planning.GetPlanning;

namespace OfficeDays.UnitTests.Features.Planning.GetPlanning;

public sealed class GetPlanningRequestValidatorTests
{
    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(9999, 12, true)]
    [InlineData(2026, 6, true)]
    [InlineData(0, 6, false)]
    [InlineData(10000, 6, false)]
    [InlineData(2026, 0, false)]
    [InlineData(2026, 13, false)]
    public void Should_ValidateYearAndMonth(int year, int month, bool expectedIsValid)
    {
        // arrange
        var request = new GetPlanningRequest { Year = year, Month = month };
        var sut = new GetPlanningRequestValidator();

        // act
        var result = sut.Validate(request);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
