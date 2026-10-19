using Microsoft.Extensions.Logging;
using OfficeDays.Features.Tokens.DeleteToken;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.DeleteToken;

public sealed class DeleteTokenHandlerLoggingTests
{
    [Fact]
    public void Should_LogTokenNotFound()
    {
        // arrange
        var expectedTokenId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<DeleteTokenHandler>();

        // act
        logger.LogTokenNotFound(expectedTokenId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Token {expectedTokenId} was not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogTokenNotRevoked()
    {
        // arrange
        var expectedTokenId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<DeleteTokenHandler>();

        // act
        logger.LogTokenNotRevoked(expectedTokenId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} cannot permanently delete API token {expectedTokenId} because it has not been revoked");
        Assert.Single(logger.Logs);
    }
}
