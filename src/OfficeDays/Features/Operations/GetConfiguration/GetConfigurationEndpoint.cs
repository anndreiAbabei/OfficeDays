using OfficeDays.Features.Operations.GetConfiguration.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations.GetConfiguration;

public sealed class GetConfigurationEndpoint(IRequestExecutor executor) : IOperationsEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("configuration", _executor.Execute<GetConfigurationRequest>);
    }
}
