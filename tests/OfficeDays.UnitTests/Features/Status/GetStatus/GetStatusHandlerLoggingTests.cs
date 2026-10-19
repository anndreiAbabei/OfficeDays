using Microsoft.Extensions.Logging;
using OfficeDays.Features.Status.GetStatus;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<GetStatusHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }
}
