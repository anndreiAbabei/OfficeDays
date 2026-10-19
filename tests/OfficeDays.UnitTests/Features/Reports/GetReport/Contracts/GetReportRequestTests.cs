using OfficeDays.Features.Reports.GetReport.Contracts;

namespace OfficeDays.UnitTests.Features.Reports.GetReport.Contracts;

public sealed class GetReportRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedYear = 21;
        var expectedStartMonth = 22;
        var expectedEndMonth = 23;

        // act
        var sut = new GetReportRequest
        {
            Year = expectedYear,
            StartMonth = expectedStartMonth,
            EndMonth = expectedEndMonth
        };

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedStartMonth, sut.StartMonth);
        Assert.Equal(expectedEndMonth, sut.EndMonth);
    }
}
