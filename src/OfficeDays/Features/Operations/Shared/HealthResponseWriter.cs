using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OfficeDays.Features.Operations.Shared;

internal static class HealthResponseWriter
{
    public static Task Write(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())
        }, context.RequestAborted);
}
