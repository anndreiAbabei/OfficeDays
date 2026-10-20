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
}
