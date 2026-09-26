using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionEndpoint(IRequestExecutor executor) : IOperationsEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("version", executor.Execute<GetVersionRequest>);
}
