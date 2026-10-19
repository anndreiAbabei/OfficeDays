using Microsoft.Extensions.Logging;
using OfficeDays.Features.Planning.PlanDate;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.PlanDate;

public sealed class PlanDateHandlerLoggingTests
{
    [Fact]
    public void Should_LogPlanChanged()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = new DateOnly(2026, 10, 2);
        var logger = new FakeLogger<PlanDateHandler>();

        // act
        logger.LogPlanChanged(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Information, $"Office plan planned for user {expectedUserId} on {expectedDate}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogResolvedConcurrentUpdate()
    {
        // arrange
        var expectedException = new InvalidOperationException("Concurrent update");
        var expectedUserId = Guid.NewGuid();
        var expectedDate = new DateOnly(2026, 10, 3);
        var logger = new FakeLogger<PlanDateHandler>();

        // act
        logger.LogResolvedConcurrentUpdate(expectedException, expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Debug, $"Resolved concurrent duplicate office plan for user {expectedUserId} on {expectedDate}", expectedException);
        Assert.Single(logger.Logs);
    }
}
