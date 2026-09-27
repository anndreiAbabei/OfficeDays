using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.RecordToday.Contracts;

public sealed record RecordTodayRequest : IRequest
{
    [FromBody]
    public RecordTodayRequestBody? Body { get; init; }
}

public sealed record RecordTodayRequestBody(bool IsManual = false);
