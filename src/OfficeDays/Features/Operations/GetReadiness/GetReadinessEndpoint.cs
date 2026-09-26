using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.Shared;

namespace OfficeDays.Features.Operations.GetReadiness;

public sealed class GetReadinessEndpoint : IHealthEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("ready", new HealthCheckOptions
        {
            ResponseWriter = HealthResponseWriter.Write
        });
}
