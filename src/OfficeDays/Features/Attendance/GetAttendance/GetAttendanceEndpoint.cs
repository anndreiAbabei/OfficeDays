using OfficeDays.Features.Attendance.GetAttendance.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.GetAttendance;

public sealed class GetAttendanceEndpoint(IHandlerCreator creator) : IAttendanceEndpoint
{
    private readonly IHandlerCreator _creator = creator;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _creator.Create<GetAttendanceRequest>);
    }
}
