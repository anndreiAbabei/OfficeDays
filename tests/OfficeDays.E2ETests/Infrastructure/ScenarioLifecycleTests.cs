using System.IO.Compression;
using System.Text.RegularExpressions;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Infrastructure;

[Collection(ApplicationCollection.Name)]
public sealed partial class ScenarioLifecycleTests : E2ETestBase
{
    public ScenarioLifecycleTests(ApplicationFixture application, ITestOutputHelper output)
        : base(application, output) {}

    [Fact]
    public async Task Should_SaveDiagnostics_WhenScenarioFails()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Intentional failure to verify diagnostics.");
        var scenarioName = $"diagnostics-{Guid.NewGuid():N}";

        // Act
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RunScenarioAsync(async (page, _, _) =>
            {
                await page.GotoAsync("/login");
                throw expectedException;
            }, true, scenarioName));

        // Assert
        Assert.Same(expectedException, actualException);
        var directory = Assert.Single(Directory.GetDirectories(Application.ArtifactsDirectory, $"{nameof(ScenarioLifecycleTests)}-{scenarioName}-*"));
        Assert.True(new FileInfo(Path.Combine(directory, "failure.png")).Length > 0);
        await using var trace = await ZipFile.OpenReadAsync(Path.Combine(directory, "trace.zip"), TestContext.Current.CancellationToken);
        Assert.Contains(trace.Entries, entry => entry.FullName.EndsWith(".trace", StringComparison.Ordinal));
        // This was an expected failure, so don't present it as a real failed scenario's artefact.
        Directory.Delete(directory, recursive: true);
    }

    [Fact]
    public async Task Should_RequireLogin_WhenStartingAnotherBrowserContext()
    {
        // Arrange
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            await login.SignIn(user);
        });

        await RunScenarioAsync(async (page, _, _) =>
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
