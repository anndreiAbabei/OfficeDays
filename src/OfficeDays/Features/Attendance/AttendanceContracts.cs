namespace OfficeDays.Features.Attendance;

public sealed record AttendanceResponse(DateOnly Date, DateTimeOffset CreatedAt);
