using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.GetAttendance.Contracts;

public sealed record GetAttendanceRequest : IRequest
{
    [FromQuery(Name = "year")]
    public int? Year { get; init; }
    
    [FromQuery(Name = "month")]
    public int? Month { get; init; }
}
