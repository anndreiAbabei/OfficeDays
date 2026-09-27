namespace OfficeDays.Features.Attendance.RecordDate;

public static partial class RecordDateHandlerLogging
{
    private const int EventIdStart = 1100;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Debug, "Ignored duplicate office attendance for user {UserId} on {AttendanceDate}")]
    public static partial void LogDuplicatedEntryRequest(this ILogger<RecordDateHandler> logger, Guid userId, DateOnly attendanceDate);
    
    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "Recorded office attendance for user {UserId} on {AttendanceDate}")]
    public static partial void LogEntryCreated(this ILogger<RecordDateHandler> logger, Guid userId, DateOnly attendanceDate);
    
    [LoggerMessage(EventIdStart + 3, LogLevel.Debug, "Resolved concurrent duplicate attendance for user {UserId} on {AttendanceDate}")]
    public static partial void LogResolvedConcurrentUpdate(this ILogger<RecordDateHandler> logger, Guid userId, DateOnly attendanceDate);
}
