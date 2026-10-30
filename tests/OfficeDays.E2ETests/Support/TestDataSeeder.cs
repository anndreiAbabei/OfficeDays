using System.Diagnostics.CodeAnalysis;
using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Models;

namespace OfficeDays.E2ETests.Support;

public interface ITestDataSeeder
{
    Task<TestUser> CreateUserAsync(CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public sealed class TestDataSeeder(IApiClient api) : ITestDataSeeder
{
    private readonly IApiClient _api = api;

    public async Task<TestUser> CreateUserAsync(CancellationToken cancellationToken = default)
    {
        const string defaultPassword = "BrowserTestsOnly!123";

        var request = new CreateUserRequest($"e2e-{Guid.NewGuid():N}",
                                            defaultPassword,
                                            "UTC",
                                            "RO");

        var user = await _api.CreateUser(request, cancellationToken);

        return new TestUser(user.Username, defaultPassword);
    }
}
