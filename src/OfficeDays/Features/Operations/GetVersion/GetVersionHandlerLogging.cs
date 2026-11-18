using OfficeDays.Services.External.GitHub;

namespace OfficeDays.Features.Operations.GetVersion;

public static partial class GetVersionHandlerLogging
{
    private const int EventIdStart = 1400;

    [LoggerMessage(EventIdStart + 1, LogLevel.Information, "Version of the app was not found in cache, retrieved {Response} from GitHub and save to {CacheKey}. " +
                                                           "Will be cached until {AbsoluteExpiration} (Effectively: {NumberOfHours})")]
    public static partial void LogCacheMiss(this ILogger<GetVersionHandler> logger, object cacheKey, GitHubGetLatestVersionSuccessResponse response, DateTimeOffset absoluteExpiration, TimeSpan numberOfHours);
    
    [LoggerMessage(EventIdStart + 2, LogLevel.Error, "Failure to retrieve the latest version from GitHub. Error {error}")]
    public static partial void LogGetLatestVersionError(this ILogger<GetVersionHandler> logger, GitHubGetLatestVersionErrorResponse error);
}
