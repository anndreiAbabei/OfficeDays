using OfficeDays.Features.Status.GetStatus.Contracts;

namespace OfficeDays.UnitTests.Features.Status.GetStatus.Contracts;

public sealed class GetStatusRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedYear = 21;
        var expectedMonth = 22;

        // act
        var sut = new GetStatusRequest
        {
            Year = expectedYear,
            Month = expectedMonth
        };

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedMonth, sut.Month);
    }
}
