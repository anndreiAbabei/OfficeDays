using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Planning;

public interface IPlanningEndpoint : IEndpoint;

public sealed class PlanningEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IPlanningEndpoint>("planning")
                 .RequireAuthorization();
    }
}
