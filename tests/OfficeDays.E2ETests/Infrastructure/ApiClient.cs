using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using OfficeDays.E2ETests.Infrastructure.Models;

namespace OfficeDays.E2ETests.Infrastructure;

public interface IApiClient : IDisposable
{
    Task EnsureLive(CancellationToken cancellationToken = default);
    Task<GetVersionResponse> GetVersion(CancellationToken cancellationToken = default);
    Task<CreateUserResponse> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };
    
    public ApiClient(string baseAddress)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseAddress),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
    
    public async Task EnsureLive(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync("/health/ready", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<GetVersionResponse> GetVersion(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync("/api/version", cancellationToken);
        
        return await Deserialize<GetVersionResponse>(response, cancellationToken);
    }
    
    public async Task<CreateUserResponse> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/users", request, cancellationToken);
        
        return await Deserialize<CreateUserResponse>(response, cancellationToken);
    }

    public void Dispose() => _http.Dispose();

    private static async ValueTask<T> Deserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions, cancellationToken) 
               ?? throw new JsonException($"Expected a {typeof(T).Name} response, but received JSON null.");
    }
}
