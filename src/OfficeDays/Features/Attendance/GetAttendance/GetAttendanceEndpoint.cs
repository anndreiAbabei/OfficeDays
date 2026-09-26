using OfficeDays.Features.Attendance.GetAttendance.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance.GetAttendance;

public sealed class GetAttendanceEndpoint(IRequestExecutor creator) : IAttendanceEndpoint
{
    private readonly IRequestExecutor _creator = creator;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _creator.Execute<GetAttendanceRequest>);
    }
}
