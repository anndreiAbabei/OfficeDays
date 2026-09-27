namespace OfficeDays.Features.Attendance.GetAttendance.Contracts;

public sealed record GetAttendanceItem(DateOnly Date, DateTimeOffset CreatedAt, bool IsManual);
