using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Attendance;

public interface IAttendanceEndpoint : IEndpoint;

public sealed class AttendanceEndpointGroups : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IAttendanceEndpoint>("attendance")
                 .RequireAuthorization();
    }
}
