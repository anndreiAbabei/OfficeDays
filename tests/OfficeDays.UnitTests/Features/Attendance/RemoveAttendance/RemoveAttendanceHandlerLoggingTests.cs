using Microsoft.Extensions.Logging;
using OfficeDays.Features.Attendance.RemoveAttendance;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.RemoveAttendance;

public sealed class RemoveAttendanceHandlerLoggingTests
{
    [Fact]
    public void Should_LogRemoveOfficeAttendance()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = DateOnly.FromDateTime(DateTime.Today);
        var logger = new FakeLogger<RemoveAttendanceHandler>();
        
        // act
        logger.LogRemoveOfficeAttendance(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Information, $"User {expectedUserId} removed office attendance for {expectedDate}");
    }
}
