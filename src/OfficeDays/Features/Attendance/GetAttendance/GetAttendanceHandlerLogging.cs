using OfficeDays.Services;

namespace OfficeDays.Features.Attendance.GetAttendance;

public static partial class GetAttendanceHandlerLogging
{
    private const int EventIdStart = 1000;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Getting attendance entries for {UserId} filtered on {Period}")]
    public static partial void LogGetEntries(this ILogger<GetAttendanceHandler> logger, Guid userId, CalculationPeriod? period);
}
