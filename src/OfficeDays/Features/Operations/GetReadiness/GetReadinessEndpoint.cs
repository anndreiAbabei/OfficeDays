using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.Shared;

namespace OfficeDays.Features.Operations.GetReadiness;

public sealed class GetReadinessEndpoint : IHealthEndpoint
{
    private static readonly HealthCheckOptions _options = new HealthCheckOptions
    {
        ResponseWriter = HealthResponseWriter.Write
    };

    public void MapEndpoint(IEndpointRouteBuilder endpoints) => endpoints.MapHealthChecks("ready", _options);
}
