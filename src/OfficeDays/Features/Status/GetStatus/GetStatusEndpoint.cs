using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusEndpoint(IRequestExecutor executor) : IStatusEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetStatusRequest>);
    }
}
