using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace OfficeDays.IntegrationTests.Support;

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
        builder.ConfigureLogging(logging => logging.AddFilter<ConsoleLoggerProvider>(level => level >= LogLevel.Critical));
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
            ["BootstrapAdmin:Password"] = "VeryStrongAdminPassword!",
            ["BootstrapAdmin:TimeZoneId"] = "Europe/Bucharest",
            ["BootstrapAdmin:CountryCode"] = "RO",
            ["BootstrapAdmin:CountryName"] = "Romania",
            ["Security:RequireHttpsForBearerTokens"] = _requireHttpsForBearerTokens.ToString()
        }));
        builder.ConfigureServices(services =>
        {
            // Capture logs through the providers configured by each test, independently of Serilog.
            services.RemoveAll<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FixedTimeProvider(new DateTimeOffset(2099, 9, 15, 9, 0, 0, TimeSpan.Zero)));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        if (disposing && File.Exists(_databasePath)) 
            File.Delete(_databasePath);
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
