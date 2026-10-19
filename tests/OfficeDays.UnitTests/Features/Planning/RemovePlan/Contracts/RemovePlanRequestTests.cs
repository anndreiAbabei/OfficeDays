using OfficeDays.Features.Planning.RemovePlan.Contracts;

namespace OfficeDays.UnitTests.Features.Planning.RemovePlan.Contracts;

public sealed class RemovePlanRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);

        // act
        var sut = new RemovePlanRequest
        {
            Date = expectedDate
        };

        // assert
        Assert.Equal(expectedDate, sut.Date);
    }
}
