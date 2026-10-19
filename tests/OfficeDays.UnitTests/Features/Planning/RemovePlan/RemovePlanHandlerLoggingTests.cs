using Microsoft.Extensions.Logging;
using OfficeDays.Features.Planning.RemovePlan;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.RemovePlan;

public sealed class RemovePlanHandlerLoggingTests
{
    [Fact]
    public void Should_LogPlanChanged()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = new DateOnly(2026, 10, 2);
        var logger = new FakeLogger<RemovePlanHandler>();

        // act
        logger.LogPlanChanged(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Information, $"Office plan removed for user {expectedUserId} on {expectedDate}");
        Assert.Single(logger.Logs);
    }
}
