using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.RemoveAttendance.Contracts;

public sealed record RemoveAttendanceRequest : IRequest
{
    [FromRoute(Name = "date")]
    public DateOnly Date { get; set; }
}
