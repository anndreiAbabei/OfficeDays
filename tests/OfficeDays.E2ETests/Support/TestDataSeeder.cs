using System.Net;
using System.Net.Http.Json;

namespace OfficeDays.E2ETests.Support;

public interface ITestDataSeeder : IDisposable
{
    Task<TestUser> CreateUserAsync(CancellationToken cancellationToken = default);
}

public sealed class TestDataSeeder : ITestDataSeeder
{
    private readonly HttpClient _client;

    public TestDataSeeder(string baseUrl)
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl), 
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<TestUser> CreateUserAsync(CancellationToken cancellationToken = default)
    {
        var user = new TestUser($"e2e-{Guid.NewGuid():N}", "BrowserTestsOnly!123");
        
        using var response = await _client.PostAsJsonAsync("/api/users", new
        {
            username = user.Username,
            password = user.Password,
            timeZoneId = "UTC",
            countryCode = "RO"
        }, cancellationToken);
        
        return response.StatusCode != HttpStatusCode.Created 
                   ? throw new InvalidOperationException($"User seeding failed with HTTP {(int)response.StatusCode}.") 
                   : user;
    }

    public void Dispose() => _client.Dispose();
}
