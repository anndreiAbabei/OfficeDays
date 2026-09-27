using System.Net;

namespace OfficeDays.Middleware;

public sealed partial class BearerTokenHttpsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BearerTokenHttpsMiddleware> _logger;
    private readonly bool _requireHttps;

    public BearerTokenHttpsMiddleware(RequestDelegate next,
                                      IConfiguration configuration,
                                      ILogger<BearerTokenHttpsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _requireHttps = configuration.GetValue("Security:RequireHttpsForBearerTokens", true);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_requireHttps && !context.Request.IsHttps && HasBearerToken(context.Request))
        {
            LogRejectInsecureBearer(_logger, context.Request.Path, context.Connection.RemoteIpAddress);
            
            await Results.Problem(statusCode: StatusCodes.Status400BadRequest,
                                  title: "HTTPS required",
                                  detail: "Bearer API tokens are not accepted over an insecure connection.")
                         .ExecuteAsync(context);
            
            return;
        }

        await _next(context);
    }

    private static bool HasBearerToken(HttpRequest request) => request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

    private const int EventIdStart = 0500;
    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "Rejected an insecure bearer request to {Path} from {IpAddress}")]
    private static partial void LogRejectInsecureBearer(ILogger<BearerTokenHttpsMiddleware> logger, string path, IPAddress? ipAddress);
}
