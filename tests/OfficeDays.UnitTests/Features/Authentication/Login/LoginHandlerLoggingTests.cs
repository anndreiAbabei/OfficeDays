using Microsoft.Extensions.Logging;
using OfficeDays.Features.Authentication.Login;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Authentication.Login;

public sealed class LoginHandlerLoggingTests
{
    [Fact]
    public void Should_LogFailedLogin()
    {
        // arrange
        var expectedUsername = "username value";
        var expectedRemoteIpAddress = System.Net.IPAddress.Loopback;
        var logger = new FakeLogger<LoginHandler>();

        // act
        logger.LogFailedLogin(expectedUsername, expectedRemoteIpAddress);

        // assert
        logger.Assert(LogLevel.Warning, $"Failed login attempt for username {expectedUsername} from {expectedRemoteIpAddress}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogSignedIn()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<LoginHandler>();

        // act
        logger.LogSignedIn(expectedUserId);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} signed in");
        Assert.Single(logger.Logs);
    }
}
