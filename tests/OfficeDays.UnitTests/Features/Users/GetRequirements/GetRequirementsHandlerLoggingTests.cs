using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.GetRequirements;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.GetRequirements;

public sealed class GetRequirementsHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<GetRequirementsHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }
}
