using OfficeDays.Features.Planning.PlanDate.Contracts;

namespace OfficeDays.UnitTests.Features.Planning.PlanDate.Contracts;

public sealed class PlanDateRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);

        // act
        var sut = new PlanDateRequest
        {
            Date = expectedDate
        };

        // assert
        Assert.Equal(expectedDate, sut.Date);
    }
}
