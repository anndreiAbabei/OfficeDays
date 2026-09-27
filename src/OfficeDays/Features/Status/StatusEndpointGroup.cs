using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Status;

public interface IStatusEndpoint : IEndpoint;

public sealed class StatusEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IStatusEndpoint>("status")
                 .RequireAuthorization();
    }
}
