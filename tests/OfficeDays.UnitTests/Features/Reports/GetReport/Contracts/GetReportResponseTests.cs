using OfficeDays.Features.Reports.GetReport.Contracts;

namespace OfficeDays.UnitTests.Features.Reports.GetReport.Contracts;

public sealed class GetReportResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedYear = 21;
        var expectedStartMonth = 22;
        var expectedEndMonth = 23;
        var expectedUsername = "Username value";
        var expectedTimeZoneId = "Europe/Bucharest";
        var expectedRegistrationMonth = new DateOnly(2026, 10, 6);
        var expectedToday = new DateOnly(2026, 10, 7);
        var expectedGeneratedAt = new DateTimeOffset(2026, 10, 8, 9, 30, 0, TimeSpan.Zero);
        var expectedSummary = new GetReportSummary(21, 43.5m, 23, 24);
        var expectedMonths = new List<GetReportMonth> { new GetReportMonth(21, 22, 23, 24, 25, 47.5m, "Result value") };

        // act
        var sut = new GetReportResponse(expectedYear, expectedStartMonth, expectedEndMonth, expectedUsername, expectedTimeZoneId, expectedRegistrationMonth, expectedToday, expectedGeneratedAt, expectedSummary, expectedMonths);

        // assert
        Assert.Equal(expectedYear, sut.Year);
        Assert.Equal(expectedStartMonth, sut.StartMonth);
        Assert.Equal(expectedEndMonth, sut.EndMonth);
        Assert.Equal(expectedUsername, sut.Username);
        Assert.Equal(expectedTimeZoneId, sut.TimeZoneId);
        Assert.Equal(expectedRegistrationMonth, sut.RegistrationMonth);
        Assert.Equal(expectedToday, sut.Today);
        Assert.Equal(expectedGeneratedAt, sut.GeneratedAt);
        Assert.Equal(expectedSummary, sut.Summary);
        Assert.Equal(expectedMonths, sut.Months);
    }
}
