using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OfficeDays.IntegrationTests;

public sealed class OfficeDaysFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"officedays-tests-{Guid.NewGuid():N}.db");
    private readonly bool _requireHttpsForBearerTokens;

    public OfficeDaysFactory(bool requireHttpsForBearerTokens = false)
    {
        _requireHttpsForBearerTokens = requireHttpsForBearerTokens;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["BootstrapAdmin:Password"] = "VeryStrongAdminPassword!",
                ["BootstrapAdmin:TimeZoneId"] = "Europe/Bucharest",
                ["Security:RequireHttpsForBearerTokens"] = _requireHttpsForBearerTokens.ToString()
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2099, 9, 15, 9, 0, 0, TimeSpan.Zero)));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_databasePath)) File.Delete(_databasePath);
    }
}

public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow() => _now;
}
