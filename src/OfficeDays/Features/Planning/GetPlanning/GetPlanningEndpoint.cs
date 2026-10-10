using OfficeDays.Features.Planning.GetPlanning.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Planning.GetPlanning;

public sealed class GetPlanningEndpoint(IRequestExecutor executor) : IPlanningEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetPlanningRequest>);
    }
}
