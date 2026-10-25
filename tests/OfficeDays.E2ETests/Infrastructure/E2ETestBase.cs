using System.Runtime.CompilerServices;
using OfficeDays.E2ETests.Support;
using Xunit.Abstractions;

namespace OfficeDays.E2ETests.Infrastructure;

public interface IE2ETest;

// Inherit only to share the application and scenario lifecycle; each scenario gets a new context.
public abstract class E2ETestBase : IE2ETest
{
    private readonly ApplicationFixture _application;
    private readonly ITestOutputHelper _output;

    protected E2ETestBase(ApplicationFixture application, ITestOutputHelper output)
    {
        _application = application;
        _output = output;
    }

    protected async Task RunScenarioAsync(Func<IPage, TestDataSeeder, Task> scenario, [CallerMemberName] string scenarioName = "Scenario")
    {
        var directory = Path.Combine(_application.ArtifactsDirectory, $"{GetType().Name}-{scenarioName}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        
        _output.WriteLine($"Artifacts: {directory}");
        
        await using var context = await _application.Browser.CreateContextAsync(_application.BaseUrl);
        await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });
        
        var page = await context.NewPageAsync();
        
        try
        {
            using var seeder = new TestDataSeeder(_application.BaseUrl);
            
            await scenario(page, seeder);
            
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
