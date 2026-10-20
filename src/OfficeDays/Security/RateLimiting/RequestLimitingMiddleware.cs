using System.Diagnostics.Metrics;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OfficeDays.Security.Logging;

namespace OfficeDays.Security.RateLimiting;

public sealed class RequestLimitingMiddleware
{
    private static readonly Meter _meter = new Meter("OfficeDays.RequestLimits");
    private static readonly Counter<long> _rejections = _meter.CreateCounter<long>("officedays.requests.rejected");
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

    public async Task InvokeAsync(HttpContext context, RequestLimitStore store)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var source = context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";
        var isAllowed = await AllowAsync(context, store, "source", source, _limits.Source);
        
        if (!isAllowed) 
            return;

        if (context.Request.ContentLength > _limits.MaxBodyBytes)
        {
            await RejectBodyAsync(context);
            
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
            
            await RejectBodyAsync(context);
            return;
        }
        
        body.Position = 0;
        context.Request.Body = body;

        try
        {
            var sensitive = context.GetEndpoint()?.Metadata.GetMetadata<SensitiveRequestLimit>();
            if (sensitive is not null)
            {
                var limit = sensitive.Policy switch
                {
                    "login" => _limits.Login,
                    "registration" => _limits.Registration,
                    "recovery" => _limits.Recovery,
                    "verification" => _limits.Verification,
                    _ => throw new InvalidOperationException("Unknown sensitive request policy.")
                };

                isAllowed = await AllowAsync(context, store, sensitive.Policy, source, limit);
                if (!isAllowed)
                    return;

                if (sensitive.AccountField is not null && body.Length > 0)
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
                                var accountLimit = sensitive.Policy == "login"
                                                       ? _limits.LoginAccount
                                                       : _limits.RecoveryAccount;

                                isAllowed = await AllowAsync(context, store, sensitive.Policy + "-account", key, accountLimit);
                                if (!isAllowed)
                                    return;
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
            }

            // Source limits run before potentially expensive bearer authentication/database work.
            var authentication = await context.AuthenticateAsync();
            if (authentication.Principal is not null)
                context.User = authentication.Principal;

            var user = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var identity = user is null ? "source:" + source : "user:" + user;

            isAllowed = await AllowAsync(context, store, "api", identity, _limits.Api);
            if (!isAllowed)
                return;

            var token = context.User.FindFirstValue("api_token_id");

            if (token is not null && !await AllowAsync(context, store, "token", token, _limits.Token))
                return;

            if (context.Request.Path.StartsWithSegments("/api/reports") && !await AllowAsync(context, store, "reports", identity, _limits.Reports))
                return;

            await _next(context);
        }
        finally
        {
            context.Request.Body = originalBody;
        }
    }

    private static Task RejectBodyAsync(HttpContext context)
    {
        _rejections.Add(1, new KeyValuePair<string, object?>("policy", "body"));
        
        return Results.Problem(statusCode: 413, title: "Request body too large").ExecuteAsync(context);
    }

    private static async Task<bool> AllowAsync(HttpContext context, RequestLimitStore store, string policy, string key, RequestLimit limit)
    {
        var retry = store.Acquire(policy + ":" + key, limit);
        
        if (retry == 0) 
            return true;
        
        _rejections.Add(1, new KeyValuePair<string, object?>("policy", policy));
        
        context.Response.Headers.RetryAfter = retry.ToString(CultureInfo.InvariantCulture);
        
        await Results.Problem(statusCode: 429, title: "Too many requests", 
                              detail: $"Too many requests. Please try again in {retry} seconds.", 
                              extensions: new Dictionary<string, object?> { ["retryAfterSeconds"] = retry })
                     .ExecuteAsync(context);
        
        return false;
    }
}