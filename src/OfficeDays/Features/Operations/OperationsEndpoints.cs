using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OfficeDays.Features.Operations;

public static class OperationsEndpoints
{
    public static void Map(WebApplication app)
    {
        var version = typeof(Program).Assembly
                                     .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                                     .InformationalVersion;
        app.MapGet("/api/version", (HttpContext context) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            
            return Results.Ok(new { version });
        }).AllowAnonymous();

        var live = new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponse
        };
        var ready = new HealthCheckOptions
        {
            ResponseWriter = WriteResponse
        };
        
        app.MapHealthChecks("/health/live", live).AllowAnonymous();
        app.MapHealthChecks("/health/ready", ready).AllowAnonymous();
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())
        };
        
        return context.Response.WriteAsJsonAsync(response, context.RequestAborted);
    }
}
