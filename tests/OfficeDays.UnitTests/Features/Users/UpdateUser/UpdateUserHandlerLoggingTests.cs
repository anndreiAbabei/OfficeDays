using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.UpdateUser;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.UpdateUser;

public sealed class UpdateUserHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<UpdateUserHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }
}
