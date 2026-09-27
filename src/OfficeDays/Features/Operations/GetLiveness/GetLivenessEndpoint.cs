using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.Shared;

namespace OfficeDays.Features.Operations.GetLiveness;

public sealed class GetLivenessEndpoint : IHealthEndpoint
{
    private static readonly HealthCheckOptions _options = new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = HealthResponseWriter.Write
    };
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => endpoints.MapHealthChecks("live", _options);
}
