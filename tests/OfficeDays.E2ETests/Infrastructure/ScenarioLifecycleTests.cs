using System.IO.Compression;
using System.Text.RegularExpressions;
using OfficeDays.E2ETests.Pages;
using Xunit.Abstractions;

namespace OfficeDays.E2ETests.Infrastructure;

[Collection(ApplicationCollection.Name)]
public sealed partial class ScenarioLifecycleTests : E2ETestBase
{
    private readonly ApplicationFixture _application;

    public ScenarioLifecycleTests(ApplicationFixture application, ITestOutputHelper output)
        : base(application, output)
    {
        _application = application;
    }

    [Fact]
    public async Task Should_SaveDiagnostics_WhenScenarioFails()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Intentional failure to verify diagnostics.");
        var scenarioName = $"diagnostics-{Guid.NewGuid():N}";

        // Act
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RunScenarioAsync(async (page, _) =>
            {
                await page.GotoAsync("/login");
                throw expectedException;
            }, scenarioName));

        // Assert
        Assert.Same(expectedException, actualException);
        var directory = Assert.Single(Directory.GetDirectories(_application.ArtifactsDirectory, $"{nameof(ScenarioLifecycleTests)}-{scenarioName}-*"));
        Assert.True(new FileInfo(Path.Combine(directory, "failure.png")).Length > 0);
        await using var trace = await ZipFile.OpenReadAsync(Path.Combine(directory, "trace.zip"));
        Assert.Contains(trace.Entries, entry => entry.FullName.EndsWith(".trace", StringComparison.Ordinal));
        // This was an expected failure, so don't present it as a real failed scenario's artefact.
        Directory.Delete(directory, recursive: true);
    }

    [Fact]
    public async Task Should_RequireLogin_WhenStartingAnotherBrowserContext()
    {
        // Arrange
        await RunScenarioAsync(async (page, seeder) =>
        {
            var user = await seeder.CreateUserAsync();
            await new LoginPage(page).SignIn(user);
        });

        await RunScenarioAsync(async (page, _) =>
        {
            // Act
            await page.GotoAsync("/profile");

            // Assert
            await Expect(page).ToHaveURLAsync(LoginRegex());
            await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true })).ToBeVisibleAsync();
        });
    }

    [GeneratedRegex("/login$")]
    private static partial Regex LoginRegex();
}
