using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.ExportData;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.ExportData;

public sealed class ExportDataHandlerLoggingTests
{
    [Fact]
    public void Should_LogUserNotFound()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var logger = new FakeLogger<ExportDataHandler>();

        // act
        logger.LogUserNotFound(expectedUserId);

        // assert
        logger.Assert(LogLevel.Warning, $"User {expectedUserId} was not found");
        Assert.Single(logger.Logs);
    }
}
