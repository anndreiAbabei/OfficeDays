using Microsoft.Extensions.Logging;
using OfficeDays.Features.Vacations.RemoveVacation;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationHandlerLoggingTests
{
    [Fact]
    public void Should_LogVacationRemoved()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedVacationId = Guid.NewGuid();
        var logger = new FakeLogger<RemoveVacationHandler>();

        // act
        logger.LogVacationRemoved(expectedUserId, expectedVacationId);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} removed vacation {expectedVacationId}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogVacationNotFound()
    {
        // arrange
        var expectedVacationId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<RemoveVacationHandler>();

        // act
        logger.LogVacationNotFound(expectedVacationId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Vacation {expectedVacationId} was not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }
}
