using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementHandlerLoggingTests
{
    [Fact]
    public void Should_LogRequirementNotFound()
    {
        // arrange
        var expectedRequirementId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<UpdateRequirementHandler>();

        // act
        logger.LogRequirementNotFound(expectedRequirementId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Attendance requirement {expectedRequirementId} was not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogBaselineChangeRejected()
    {
        // arrange
        var expectedRequirementId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<UpdateRequirementHandler>();

        // act
        logger.LogBaselineChangeRejected(expectedRequirementId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Rejected a change to the initial attendance requirement {expectedRequirementId} for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogDuplicateRequirement()
    {
        // arrange
        var expectedException = new InvalidOperationException("Concurrent update");
        var expectedUserId = Guid.NewGuid();
        var expectedStartingFrom = new DateOnly(2026, 10, 3);
        var logger = new FakeLogger<UpdateRequirementHandler>();

        // act
        logger.LogDuplicateRequirement(expectedException, expectedUserId, expectedStartingFrom);

        // assert
        logger.Assert(LogLevel.Warning, $"An attendance requirement already exists for user {expectedUserId} starting {expectedStartingFrom}", expectedException);
        Assert.Single(logger.Logs);
    }
}
