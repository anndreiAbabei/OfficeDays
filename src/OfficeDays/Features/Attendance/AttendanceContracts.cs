namespace OfficeDays.Features.Attendance;

public sealed record RecordAttendanceRequest(bool IsManual = false);

public sealed record AttendanceResponse(DateOnly Date, DateTimeOffset CreatedAt, bool IsManual);
