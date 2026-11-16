using System.Net;
using System.Text.Json;
using OfficeDays.Services.External.GitHub.Models;

namespace OfficeDays.Services.External.GitHub;

public interface IGitHubClient
{
    Task<GitHubGetLatestVersionResponse> GetLatestVersion(CancellationToken cancellationToken);
}

public sealed class GitHubClient : IGitHubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubClient> _logger;

    public GitHubClient(HttpClient httpClient, ILogger<GitHubClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<GitHubGetLatestVersionResponse> GetLatestVersion(CancellationToken cancellationToken)
    {
        var uri = new Uri("/releases/latest", UriKind.Relative);

        using var response = await _httpClient.GetAsync(uri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogFailureGetLatestVersion(uri, response.StatusCode, result);
            
            return new GitHubGetLatestVersionErrorResponse(result);
        }
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var versionResponse = await JsonSerializer.DeserializeAsync<GetLatestVersionResponse>(stream, cancellationToken: cancellationToken);

        if (versionResponse == null)
        {
            _logger.LogNullGetLatestVersion(uri);
            
            return new GitHubGetLatestVersionErrorResponse("Fail to get latest version");
        }
        
        _logger.LogSuccessGetLatestVersion(uri, response.StatusCode, versionResponse);
        
        var latestVersion = versionResponse.Name;
        
        _logger.LogSuccessGetLatestVersionReturn(uri, latestVersion);
        
        return new GitHubGetLatestVersionSuccessResponse(latestVersion);
    }
}

internal static partial class GitHubClientLoggingEx
{
    private const int EventIdStart = 10100;
    
    [LoggerMessage(EventIdStart + 1, LogLevel.Error, "Fail to get latest version from GET {RequestUri}. Error: {StatusCode} {ErrorMessage}")]
    internal static partial void LogFailureGetLatestVersion(this ILogger logger, Uri requestUri, HttpStatusCode statusCode, string errorMessage);
    
    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "Got latest version from GET {RequestUri}. Response: {StatusCode} {Response}")]
    internal static partial void LogSuccessGetLatestVersion(this ILogger logger, Uri requestUri, HttpStatusCode statusCode, GetLatestVersionResponse response);
    
    [LoggerMessage(EventIdStart + 3, LogLevel.Error, "Null response from GET {RequestUri}")]
    internal static partial void LogNullGetLatestVersion(this ILogger logger, Uri requestUri);
    
    [LoggerMessage(EventIdStart + 4, LogLevel.Information, "Returning {Version} from GET {RequestUri}")]
    internal static partial void LogSuccessGetLatestVersionReturn(this ILogger logger, Uri requestUri, string version);

}