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
        var uri = new Uri("releases/latest", UriKind.Relative);
        try
        {
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogFailureGetLatestVersion(uri, response.StatusCode, error);
                
                return new GitHubGetLatestVersionErrorResponse($"GitHub returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var release = await JsonSerializer.DeserializeAsync(stream, GetLatestVersionResponseContext.Default.GetLatestVersionResponse, cancellationToken);
            
            var tag = release?.TagName;
            if (string.IsNullOrWhiteSpace(tag))
            {
                _logger.LogNullGetLatestVersion(uri);
                
                return new GitHubGetLatestVersionErrorResponse("GitHub returned a release without a tag.");
            }

            var version = tag.StartsWith('v') ? tag[1..] : tag;
            if (!Version.TryParse(version, out _))
            {
                _logger.LogInvalidVersion(uri, version);
                
                return new GitHubGetLatestVersionErrorResponse("GitHub returned an invalid release version.");
            }

            _logger.LogSuccessGetLatestVersionReturn(uri, version);
            
            return new GitHubGetLatestVersionSuccessResponse(version);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogTimeoutError(ex, uri);
            
            return new GitHubGetLatestVersionErrorResponse("GitHub request timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogHttpRequestException(ex, uri);

            return new GitHubGetLatestVersionErrorResponse(ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogJsonException(ex, uri);

            return new GitHubGetLatestVersionErrorResponse("GitHub returned an invalid release document.");
        }
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
    
    [LoggerMessage(EventIdStart + 5, LogLevel.Error, "Invalid version recieved for GET {RequestUri}: {Version}")]
    internal static partial void LogInvalidVersion(this ILogger logger, Uri requestUri, string version);
    
    [LoggerMessage(EventIdStart + 6, LogLevel.Warning, "GitHub request timed out GET {RequestUri}")]
    internal static partial void LogTimeoutError(this ILogger logger, OperationCanceledException exception, Uri requestUri);
    
    [LoggerMessage(EventIdStart + 7, LogLevel.Error, "GitHub HttpRequestException timed out for GET {RequestUri}")]
    internal static partial void LogHttpRequestException(this ILogger logger, HttpRequestException exception, Uri requestUri);
    
    [LoggerMessage(EventIdStart + 8, LogLevel.Error, "GitHub returned an invalid release document for GET {RequestUri}")]
    internal static partial void LogJsonException(this ILogger logger, JsonException exception, Uri requestUri);

}