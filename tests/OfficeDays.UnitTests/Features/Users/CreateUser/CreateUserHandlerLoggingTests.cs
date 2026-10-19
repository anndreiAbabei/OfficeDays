using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.CreateUser;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.CreateUser;

public sealed class CreateUserHandlerLoggingTests
{
    [Fact]
    public void Should_LogDuplicateUsername()
    {
        // arrange
        var expectedUsername = "username value";
        var logger = new FakeLogger<CreateUserHandler>();

        // act
        logger.LogDuplicateUsername(expectedUsername);

        // assert
        logger.Assert(LogLevel.Warning, $"A duplicate username registration was attempted for {expectedUsername}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogUserCreated()
    {
        // arrange
        var expectedUsername = "username value";
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<CreateUserHandler>();

        // act
        logger.LogUserCreated(expectedUsername, expectedUserId);

        // assert
        logger.Assert(LogLevel.Information, $"Created normal user {expectedUsername} with user ID {expectedUserId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogJurisdictionNotFound()
    {
        // arrange
        var expectedJurisdictionCode = "jurisdictionCode value";
        var logger = new FakeLogger<CreateUserHandler>();

        // act
        logger.LogJurisdictionNotFound(expectedJurisdictionCode);

        // assert
        logger.Assert(LogLevel.Warning, $"Holiday jurisdiction {expectedJurisdictionCode} was not found");
        Assert.Single(logger.Logs);
    }
}
