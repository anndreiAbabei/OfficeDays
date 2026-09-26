using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.Shared;

namespace OfficeDays.Features.Operations.GetLiveness;

public sealed class GetLivenessEndpoint : IHealthEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthResponseWriter.Write
        });
}
