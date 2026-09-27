using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionEndpoint(IRequestExecutor executor) : IOperationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => endpoints.MapGet("version", _executor.Execute<GetVersionRequest>);
}
