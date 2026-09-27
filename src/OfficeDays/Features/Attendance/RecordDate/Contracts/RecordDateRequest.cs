using Microsoft.AspNetCore.Mvc;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.RecordDate.Contracts;

public sealed record RecordDateRequest : IRequest
{
    [FromRoute(Name = "date")]
    public required DateOnly Date { get; init; }
}
