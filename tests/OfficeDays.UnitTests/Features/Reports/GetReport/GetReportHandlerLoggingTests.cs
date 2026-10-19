using Microsoft.Extensions.Logging;
using OfficeDays.Features.Reports.GetReport;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Reports.GetReport;

public sealed class GetReportHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<GetReportHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogReportGenerated()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedYear = 22;
        var expectedStartMonth = 23;
        var expectedEndMonth = 24;
        var expectedMonthCount = 25;
        var logger = new FakeLogger<GetReportHandler>();

        // act
        logger.LogReportGenerated(expectedUserId, expectedYear, expectedStartMonth, expectedEndMonth, expectedMonthCount);

        // assert
        logger.Assert(LogLevel.Information, $"Generated attendance report for user {expectedUserId}, year {expectedYear}, months {expectedStartMonth} to {expectedEndMonth}, with {expectedMonthCount} rows");
        Assert.Single(logger.Logs);
    }
}
