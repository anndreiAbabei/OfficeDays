namespace OfficeDays.Features.Attendance.RemoveAttendance;

public static partial class RemoveAttendanceHandlerLogging
{
    private const int EventIdStart = 1300;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "User {UserId} removed office attendance for {AttendanceDate}")]
    public static partial void LogRemoveOfficeAttendance(this ILogger<RemoveAttendanceHandler> logger, Guid userId, DateOnly attendanceDate);
}
