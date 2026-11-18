using System.Diagnostics;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using OfficeDays.Services;
using OfficeDays.Services.External.GitHub;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionHandler : IRequestHandler<GetVersionRequest>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IAppVersionService _appVersion;
    private readonly IMemoryCache _memoryCache;
    private readonly IGitHubClient _gitHubClient;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GetVersionHandler> _logger;

    public GetVersionHandler(IHttpContextAccessor contextAccessor, 
                             IAppVersionService appVersion,
                             IMemoryCache memoryCache,
                             IGitHubClient gitHubClient,
                             TimeProvider timeProvider, 
                             ILogger<GetVersionHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _appVersion = appVersion;
        _memoryCache = memoryCache;
        _gitHubClient = gitHubClient;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(GetVersionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");

        context.Response.Headers.CacheControl = "no-store";

        var latestVersion = await GetLatestVersion(cancellationToken);

        var (version, author, repo) = _appVersion.GetAll();

        var response = new GetVersionResponse(version, latestVersion, author, repo);

        return Results.Ok(response);
    }
    
    private async ValueTask<string> GetLatestVersion(CancellationToken cancellationToken)
    {
        const string latestVersionKey = "App.LatestVersion";

        return await _memoryCache.GetOrCreateAsync(latestVersionKey, async ce => await GetLatestVersion(ce, cancellationToken)) 
               ?? throw new UnreachableException("Something went really wrong");
    }
    
    private async Task<string> GetLatestVersion(ICacheEntry cacheEntry, CancellationToken cancellationToken)
    {
        var githubResponse = await _gitHubClient.GetLatestVersion(cancellationToken);

        return githubResponse! switch
        {
            GitHubGetLatestVersionSuccessResponse success => GetSuccess(success),
            GitHubGetLatestVersionErrorResponse error => GetError(error)
        };

        string GetSuccess(GitHubGetLatestVersionSuccessResponse success)
        {
            var expiration = TimeSpan.FromHours(24);
            var absoluteExpiration = _timeProvider.GetUtcNow().Add(expiration);

            _logger.LogCacheMiss(cacheEntry.Key, success, absoluteExpiration, expiration);
        
            cacheEntry.SetAbsoluteExpiration(absoluteExpiration);

            return success.Version;
        }
        
        string GetError(GitHubGetLatestVersionErrorResponse error)
        {
            _logger.LogGetLatestVersionError(error);

            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            
            return _appVersion.Version;
        }
    }
}
