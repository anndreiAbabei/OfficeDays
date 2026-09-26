using OfficeDays.Features.Attendance.RecordDate.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RecordDate;

public sealed class RecordDateEndpoint(IHandlerCreator creator) : IAttendanceEndpoint
{
    private readonly IHandlerCreator _creator = creator;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("{date}", _creator.Create<RecordDateRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
