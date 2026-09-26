using OfficeDays.Features.Attendance.RecordToday.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RecordToday;

public sealed class RecordTodayEndpoint(IHandlerCreator creator) : IAttendanceEndpoint
{
    private readonly IHandlerCreator _creator = creator;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _creator.Create<RecordTodayRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
