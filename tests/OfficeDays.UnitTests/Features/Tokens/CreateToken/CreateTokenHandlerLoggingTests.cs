using Microsoft.Extensions.Logging;
using OfficeDays.Features.Tokens.CreateToken;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenHandlerLoggingTests
{
    [Fact]
    public void Should_LogTokenCreated()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedTokenId = Guid.NewGuid();
        var expectedTokenName = "tokenName value";
        var logger = new FakeLogger<CreateTokenHandler>();

        // act
        logger.LogTokenCreated(expectedUserId, expectedTokenId, expectedTokenName);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} created API token {expectedTokenId} named {expectedTokenName}");
        Assert.Single(logger.Logs);
    }
}
