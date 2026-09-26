namespace OfficeDays.Features.Attendance.RecordToday.Contracts;

public sealed record RecordTodayResponse(DateOnly Date, DateTimeOffset CreatedAt, bool IsManual);