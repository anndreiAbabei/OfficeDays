using System.Net;
using OfficeDays.Services.External.GitHub;
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

    private readonly bool _useApplicationLogging;

    public HttpStatusCode ReleaseStatus { get; init; } = HttpStatusCode.OK;
    public Uri? ReleaseRequestUri { get; private set; }
    public string? ReleaseUserAgent { get; private set; }

    public OfficeDaysFactory(bool requireHttpsForBearerTokens = false, bool useApplicationLogging = false)
    {
        _requireHttpsForBearerTokens = requireHttpsForBearerTokens;
        _useApplicationLogging = useApplicationLogging;
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
            services.AddHttpClient<IGitHubClient, GitHubClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new ReleaseHandler(this));

            // Capture logs through the providers configured by each test, independently of Serilog.
            if (!_useApplicationLogging)
            {
                services.RemoveAll<ILoggerFactory>();
                services.AddSingleton<ILoggerFactory, LoggerFactory>();
            }
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
    private sealed class ReleaseHandler(OfficeDaysFactory factory) : HttpMessageHandler
    {
        private readonly OfficeDaysFactory _factory = factory;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _factory.ReleaseRequestUri = request.RequestUri;
            _factory.ReleaseUserAgent = request.Headers.UserAgent.ToString();
            return Task.FromResult(new HttpResponseMessage(_factory.ReleaseStatus)
            {
                Content = new StringContent("""{"tag_name":"v999.0.0","name":"Autumn release"}""")
            });
        }
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
