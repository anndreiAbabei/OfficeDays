using Microsoft.Extensions.Logging;
using OfficeDays.Features.Planning.GetPlanning;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.GetPlanning;

public sealed class GetPlanningHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<GetPlanningHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }
}
