using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OfficeDays.E2ETests.Support;

namespace OfficeDays.E2ETests.Infrastructure;

public delegate Task RunScenarioHandler(IPage page, TestDataSeeder seeder, CancellationToken cancellationToken = default);

public interface IE2ETest;

// Inherit only to share the application and scenario lifecycle; each scenario gets a new context.
[ExcludeFromCodeCoverage]
public abstract class E2ETestBase : IE2ETest
{
    private readonly ITestOutputHelper _output;
    private readonly ApplicationFixture _application;
    protected IApplicationFixture Application => _application;

    protected E2ETestBase(ApplicationFixture application, ITestOutputHelper output)
    {
        _application = application;
        _output = output;
    }

    protected async Task RunScenarioAsync(RunScenarioHandler scenario, bool headless = true, [CallerMemberName] string scenarioName = "Scenario")
    {
        var directory = Path.Combine(_application.ArtifactsDirectory, $"{GetType().Name}-{scenarioName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        
        _output.WriteLine($"Artifacts: {directory}");
        
        await using var context = await Application.Browser.CreateContextAsync(_application.BaseUrl, !headless);
        await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });
        
        var page = await context.NewPageAsync();
        var ct = Application.CancellationToken;
        
        try
        {
            var seeder = new TestDataSeeder(Application.Api);
            
            await scenario(page, seeder, ct);
            
            await context.Tracing.StopAsync();
        }
        catch
        {
            try
            {
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(directory, "failure.png"), FullPage = true, Timeout = 5000 });
            }
            catch (Exception error)
            {
                _output.WriteLine($"Screenshot unavailable: {error.Message}");
            }
            try
            {
                await context.Tracing.StopAsync(new TracingStopOptions { Path = Path.Combine(directory, "trace.zip") });
            }
            catch (Exception error)
            {
                _output.WriteLine($"Trace unavailable: {error.Message}");
            }
            throw;
        }
    }
}
