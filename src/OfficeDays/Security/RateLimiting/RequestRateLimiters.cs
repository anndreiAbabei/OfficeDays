using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace OfficeDays.Security.RateLimiting;

/// <summary>Composes framework limiters; counters, synchronisation and replenishment belong to .NET.</summary>
public sealed class RequestRateLimiters : IDisposable
{
    private readonly RequestLimits _limits;
    public static readonly object AccountKey = new object();
    public static readonly object PolicyKey = new object();

    public PartitionedRateLimiter<HttpContext> Source { get; }
    public PartitionedRateLimiter<HttpContext> Api { get; }

    public RequestRateLimiters(IOptions<RequestLimits> options)
    {
        _limits = options.Value;
        Source = Create("source", context => new RequestLimiterCreateData(ClientSource(context), _limits.Source));
        Api = PartitionedRateLimiter.CreateChained(CreateSensitiveSource(),
                                                   CreateAccountLimiter(),
                                                   CreateApiLimiter(),
                                                   CreateTokenLimiter(),
                                                   CreateReportsLimiter());
    }
    
    private PartitionedRateLimiter<HttpContext> CreateSensitiveSource()
    {
        return Create("sensitive-source", context =>
        {
            var policy = context.GetEndpoint()?.Metadata
                                .GetMetadata<SensitiveRequestLimit>()?
                                .Policy;
            
            return policy switch
            {
                "login"        => CreateRequestLimiter(_limits.Login),
                "registration" => CreateRequestLimiter(_limits.Registration),
                "recovery"     => CreateRequestLimiter(_limits.Recovery),
                "verification" => CreateRequestLimiter(_limits.Verification),
                null           => null,
                _              => throw new InvalidOperationException("Unknown sensitive request policy.")
            };

            RequestLimiterCreateData CreateRequestLimiter(RequestLimit limit) => new RequestLimiterCreateData(policy + ":" + ClientSource(context), limit);
        });
        
    }
    
    private PartitionedRateLimiter<HttpContext> CreateAccountLimiter()
    {
        return Create("account", context =>
        {
            if (context.Items[AccountKey] is not string account) 
                return null;
            
            var policy = context.GetEndpoint()?.Metadata.GetMetadata<SensitiveRequestLimit>()?.Policy;
            
            return new RequestLimiterCreateData(policy + ":" + account,
                                                policy == "login"
                                                    ? _limits.LoginAccount
                                                    : _limits.RecoveryAccount);
        });
    }
    
    private PartitionedRateLimiter<HttpContext> CreateApiLimiter()
    {
        return Create("api", context => new RequestLimiterCreateData(Identity(context), _limits.Api));
    }
    
    private PartitionedRateLimiter<HttpContext> CreateTokenLimiter()
    {
        return Create("token", context => context.User.FindFirstValue("api_token_id") is { } token
                                              ? new RequestLimiterCreateData(token, _limits.Token) 
                                              : null);
    }
    
    private PartitionedRateLimiter<HttpContext> CreateReportsLimiter()
    {
        return Create("reports", context => context.Request.Path.StartsWithSegments("/api/reports")
                                                ? new RequestLimiterCreateData(Identity(context), _limits.Reports) 
                                                : null);
    }

    private static string ClientSource(HttpContext context) => context.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "unknown";

    private static string Identity(HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.NameIdentifier) is {} user
                   ? "user:" + user
                   : "source:" + ClientSource(context);
    }

    private sealed record RequestLimiterCreateData(string Key, RequestLimit Limit);

    private delegate RequestLimiterCreateData? RequestLimiterCreateHandler(HttpContext context);

    private static PartitionedRateLimiter<HttpContext> Create(string policy, RequestLimiterCreateHandler partition)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api") || partition(context) is not {} settings)
                return RateLimitPartition.GetNoLimiter("exempt");

            context.Items[PolicyKey] = policy;
            
            return RateLimitPartition.GetFixedWindowLimiter(settings.Key, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = settings.Limit.Permits,
                Window = TimeSpan.FromSeconds(settings.Limit.WindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });
    }

    public void Dispose()
    {
        Source.Dispose();
        Api.Dispose();
    }
}
