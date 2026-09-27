using OfficeDays.Features.Attendance.RecordToday.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Attendance.RecordToday;

public sealed class RecordTodayEndpoint(IRequestExecutor creator) : IAttendanceEndpoint
{
    private readonly IRequestExecutor _creator = creator;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _creator.Execute<RecordTodayRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
