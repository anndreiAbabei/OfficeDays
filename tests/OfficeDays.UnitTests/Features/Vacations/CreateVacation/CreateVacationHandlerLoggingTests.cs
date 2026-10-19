using Microsoft.Extensions.Logging;
using OfficeDays.Features.Vacations.CreateVacation;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Vacations.CreateVacation;

public sealed class CreateVacationHandlerLoggingTests
{
    [Fact]
    public void Should_LogVacationCreated()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedVacationId = Guid.NewGuid();
        var expectedFromDate = new DateOnly(2026, 10, 3);
        var expectedToDate = new DateOnly(2026, 10, 4);
        var logger = new FakeLogger<CreateVacationHandler>();

        // act
        logger.LogVacationCreated(expectedUserId, expectedVacationId, expectedFromDate, expectedToDate);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} added vacation {expectedVacationId} from {expectedFromDate} to {expectedToDate}");
        Assert.Single(logger.Logs);
    }
}
