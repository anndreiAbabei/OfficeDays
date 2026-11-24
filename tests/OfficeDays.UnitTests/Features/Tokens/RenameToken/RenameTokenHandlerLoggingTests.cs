using Microsoft.Extensions.Logging;
using OfficeDays.Features.Tokens.RenameToken;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken;

public sealed class RenameTokenHandlerLoggingTests
{
    [Fact]
    public void Should_LogWarning_WhenTokenIsNotFound()
    {
        // arrange
        var expectedTokenId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<RenameTokenHandler>();

        // act
        logger.LogTokenNotFound(expectedTokenId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Token {expectedTokenId} was not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogWarning_WhenRenameIsRejectedForRevokedToken()
    {
        // Arrange
        const int expectedEventId = 4902;
        var tokenId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var logger = new FakeLogger<RenameTokenHandler>();

        // Act
        logger.LogRenameRejectedForRevokedToken(tokenId, userId);

        // Assert
        logger.Assert(LogLevel.Warning, $"Rename rejected for revoked token {tokenId} owned by user {userId}");
        Assert.Equal(expectedEventId, Assert.Single(logger.Logs).EventId.Id);
    }
}
