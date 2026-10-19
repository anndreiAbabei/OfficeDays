using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.CreateRequirement;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.CreateRequirement;

public sealed class CreateRequirementHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<CreateRequirementHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogDuplicateRequirement()
    {
        // arrange
        var expectedException = new InvalidOperationException("Concurrent update");
        var expectedUserId = Guid.NewGuid();
        var expectedStartingFrom = new DateOnly(2026, 10, 3);
        var logger = new FakeLogger<CreateRequirementHandler>();

        // act
        logger.LogDuplicateRequirement(expectedException, expectedUserId, expectedStartingFrom);

        // assert
        logger.Assert(LogLevel.Warning, $"An attendance requirement already exists for user {expectedUserId} starting {expectedStartingFrom}", expectedException);
        Assert.Single(logger.Logs);
    }
}
