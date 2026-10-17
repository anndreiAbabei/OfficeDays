using Microsoft.Extensions.Logging;
using OfficeDays.Features.Attendance.GetAttendance;
using OfficeDays.Services;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.GetAttendance;

public sealed class GetAttendanceHandlerLoggingTesting
{
    [Fact]
    public void Should_LogGetEntries()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedPeriod = new CalculationPeriod(DateOnly.FromDateTime(DateTime.Today), 
                                                   DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                                                   "test");
        var logger = new FakeLogger<GetAttendanceHandler>();

        // act
        logger.LogGetEntries(expectedUserId, expectedPeriod);

        // assert
        logger.Assert(LogLevel.Information, $"Getting attendance entries for {expectedUserId} filtered on {expectedPeriod}");
    }
}
