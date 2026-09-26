using OfficeDays.Features.Attendance.RemoveAttendance.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RemoveAttendance;

public sealed class RemoveAttendanceEndpoint(IHandlerCreator creator) : IAttendanceEndpoint
{
    private readonly IHandlerCreator _creator = creator;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{date}", _creator.Create<RemoveAttendanceRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
