using OfficeDays.Features.Attendance.RemoveAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RemoveAttendance;

public sealed class RemoveAttendanceEndpoint(IRequestExecutor creator) : IAttendanceEndpoint
{
    private readonly IRequestExecutor _creator = creator;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{date}", _creator.Execute<RemoveAttendanceRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
