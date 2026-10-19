using OfficeDays.Features.Vacations.GetVacations.Contracts;

namespace OfficeDays.UnitTests.Features.Vacations.GetVacations.Contracts;

public sealed class GetVacationsRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedYear = 21;
        var expectedMonth = 22;

        // act
        var sut = new GetVacationsRequest
        {
            Year = expectedYear,
            Month = expectedMonth
        };

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedMonth, sut.Month);
    }
}
