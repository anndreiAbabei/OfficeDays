using Microsoft.Extensions.Logging;
using OfficeDays.Features.Tokens.RevokeToken;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.RevokeToken;

public sealed class RevokeTokenHandlerLoggingTests
{
    [Fact]
    public void Should_LogTokenRevoked()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedTokenId = Guid.NewGuid();
        var logger = new FakeLogger<RevokeTokenHandler>();

        // act
        logger.LogTokenRevoked(expectedUserId, expectedTokenId);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} revoked API token {expectedTokenId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogTokenNotFound()
    {
        // arrange
        var expectedTokenId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<RevokeTokenHandler>();

        // act
        logger.LogTokenNotFound(expectedTokenId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Token {expectedTokenId} was not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }
}
