using Microsoft.Extensions.Logging;
using OfficeDays.Features.Authentication.Logout;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Authentication.Logout;

public sealed class LogoutHandlerLoggingTests
{
    [Fact]
    public void Should_LogSignedOut()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<LogoutHandler>();

        // act
        logger.LogSignedOut(expectedUserId);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} signed out");
        Assert.Single(logger.Logs);
    }
}
