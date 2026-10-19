using Microsoft.Extensions.Logging;
using OfficeDays.Features.Vacations.UpdateVacation;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.UpdateVacation;

public sealed class UpdateVacationHandlerLoggingTests
{
    [Fact]
    public void Should_LogVacationUpdated()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedVacationId = Guid.NewGuid();
        var expectedFromDate = new DateOnly(2026, 10, 3);
        var expectedToDate = new DateOnly(2026, 10, 4);
        var logger = new FakeLogger<UpdateVacationHandler>();

        // act
        logger.LogVacationUpdated(expectedUserId, expectedVacationId, expectedFromDate, expectedToDate);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} updated vacation {expectedVacationId} from {expectedFromDate} to {expectedToDate}");
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void Should_LogVacationNotFound()
    {
        // arrange
        var expectedVacationId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<UpdateVacationHandler>();

        // act
        logger.LogVacationNotFound(expectedVacationId, expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"Vacation {expectedVacationId} not found for user {expectedUserId}");
        Assert.Single(logger.Logs);
    }
}
