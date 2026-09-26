using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.RecordToday.Contracts;

public sealed record RecordTodayRequest(bool IsManual = false) : IRequest;
