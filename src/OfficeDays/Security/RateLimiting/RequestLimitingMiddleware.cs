using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OfficeDays.Security.Logging;

namespace OfficeDays.Security.RateLimiting;

public sealed class RequestLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestLimits _limits;
    private readonly ILogger<RequestLimitingMiddleware> _logger;
    
    public RequestLimitingMiddleware(RequestDelegate next, 
                                     IOptions<RequestLimits> options,
                                     ILogger<RequestLimitingMiddleware> logger)
    {
        _next = next;
        _limits = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestRateLimiters limiters)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            
            return;
        }

        // This framework limiter runs before body parsing and authentication/database work.
        using var lease = limiters.Source.AttemptAcquire(context);
        if (!lease.IsAcquired)
        {
            await RequestLimitResponses.RejectAsync(context, lease);
            
            return;
        }

        if (context.Request.ContentLength > _limits.MaxBodyBytes)
        {
            await RequestLimitResponses.RejectBodyAsync(context);
            
            return;
        }

        // Buffer at most the configured limit, including bodies without Content-Length.
        using var body = new MemoryStream();
        var originalBody = context.Request.Body;
        var buffer = new byte[8192];
        
        while (true)
        {
            var memory = buffer.AsMemory(0, Math.Min(buffer.Length, _limits.MaxBodyBytes + 1 - (int)body.Length));
            var count = await originalBody.ReadAsync(memory, context.RequestAborted);
            
            if (count == 0) 
                break;
            
            await body.WriteAsync(buffer.AsMemory(0, count), context.RequestAborted);
            
            if (body.Length <= _limits.MaxBodyBytes)
                continue;
            
            await RequestLimitResponses.RejectBodyAsync(context);
            
            return;
        }
        
        body.Position = 0;
        context.Request.Body = body;

        try
        {
            var sensitive = context.GetEndpoint()?.Metadata.GetMetadata<SensitiveRequestLimit>();
            if (sensitive?.AccountField != null && body.Length > 0)
            {
                try
                {
                    using var json = await JsonDocument.ParseAsync(body, cancellationToken: context.RequestAborted);
                    if (json.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        // Minimal API JSON property matching is case-insensitive too.
                        var account = json.RootElement.EnumerateObject().LastOrDefault(x => x.Name.Equals(sensitive.AccountField, StringComparison.OrdinalIgnoreCase)).Value;
                        if (account.ValueKind == JsonValueKind.String)
                        {
                            var normalized = account.GetString()?.Trim().ToUpperInvariant() ?? "";
                            var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
                            context.Items[RequestRateLimiters.AccountKey] = key;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogJsonExceptionError(ex);
                }
                finally
                {
                    body.Position = 0;
                }
            }

            await _next(context);
        }
        finally
        {
            context.Request.Body = originalBody;
        }
    }

}
