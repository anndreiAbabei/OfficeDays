namespace OfficeDays.Features.Attendance.RecordDate.Contracts;

public sealed record RecordDateResponse(DateOnly Date, DateTimeOffset CreatedAt, bool IsManual);