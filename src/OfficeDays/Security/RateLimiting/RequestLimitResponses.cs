using System.Diagnostics.Metrics;
using System.Globalization;
using System.Threading.RateLimiting;

namespace OfficeDays.Security.RateLimiting;

public static class RequestLimitResponses
{
    private static readonly Meter _meter = new Meter("OfficeDays.RequestLimits");
    private static readonly Counter<long> _rejections = _meter.CreateCounter<long>("officedays.requests.rejected");

    public static Task RejectBodyAsync(HttpContext context)
    {
        _rejections.Add(1, new KeyValuePair<string, object?>("policy", "body"));
        
        return Results.Problem(statusCode: 413, title: "Request body too large")
                      .ExecuteAsync(context);
    }

    public static Task RejectAsync(HttpContext context, RateLimitLease lease)
    {
        var retry = lease.TryGetMetadata(MetadataName.RetryAfter, out var delay) 
                        ? Math.Max(1, (int)Math.Ceiling(delay.TotalSeconds)) 
                        : 1;
        
        var policy = context.Items[RequestRateLimiters.PolicyKey] as string ?? "api";
        _rejections.Add(1, new KeyValuePair<string, object?>("policy", policy));
        
        context.Response.Headers.RetryAfter = retry.ToString(CultureInfo.InvariantCulture);
        
        return Results.Problem(statusCode: 429, title: "Too many requests", 
                               detail: $"Too many requests. Please try again in {retry} seconds.", 
                               extensions: new Dictionary<string, object?> { ["retryAfterSeconds"] = retry })
                      .ExecuteAsync(context);
    }
}
