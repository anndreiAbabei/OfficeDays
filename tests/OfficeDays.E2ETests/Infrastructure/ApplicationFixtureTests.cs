using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Versioning;

namespace OfficeDays.E2ETests.Infrastructure;

public sealed class ApplicationFixtureTests : IAsyncLifetime
{
    private readonly string _repositoryRoot = Path.Combine(Path.GetTempPath(), $"officedays-fixture-tests-{Guid.NewGuid():N}");

    public ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(_repositoryRoot);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Should_AllowRepeatedCleanup_WhenApplicationHasNotInitialized()
    {
        // Arrange
        var application = new ApplicationFixture(_repositoryRoot);

        // Act
        await application.DisposeAsync();
        await application.DisposeAsync();

        // Assert
        Assert.Empty(application.BaseUrl);
        Assert.Empty(Directory.GetFileSystemEntries(_repositoryRoot));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [SuppressMessage("ReSharper", "DisposeOnUsingVariable", Justification = "part of the test")]
    public async Task Should_ReportMissingBuildOutputs_WhenInitializationFails(bool backendExists, bool frontendExists)
    {
        // Arrange
        var frameworkAttribute = typeof(ApplicationFixture).Assembly.GetCustomAttribute<TargetFrameworkAttribute>();
        var framework = new FrameworkName(Assert.IsType<TargetFrameworkAttribute>(frameworkAttribute).FrameworkName);
        var expectedTargetFramework = $"net{framework.Version.Major}.{framework.Version.Minor}";
        var configuration = Environment.GetEnvironmentVariable("E2E_CONFIGURATION") ?? "Debug";
        var appDirectory = Path.Combine(_repositoryRoot, "src", "OfficeDays");
        var assembly = Path.Combine(appDirectory, "bin", configuration, expectedTargetFramework, "OfficeDays.dll");
        var frontend = Path.Combine(appDirectory, "wwwroot", "index.html");
        var expectedMissingPath = backendExists ? frontend : assembly;
        const string expectedMessage = "Build the frontend and backend first.";
        var cancellationToken = TestContext.Current.CancellationToken;

        if (backendExists)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assembly) ?? throw new InvalidOperationException("Missing assembly directory."));
            await File.WriteAllTextAsync(assembly, string.Empty, cancellationToken);
        }

        if (frontendExists)
        {
            Directory.CreateDirectory(Path.Combine(appDirectory, "wwwroot"));
            await File.WriteAllTextAsync(frontend, string.Empty, cancellationToken);
        }

        await using var application = new ApplicationFixture(_repositoryRoot);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await application.InitializeAsync());
        await application.DisposeAsync();

        // Assert
        Assert.Contains(expectedMessage, exception.Message);
        Assert.Contains(expectedMissingPath, exception.Message);
        Assert.Empty(application.BaseUrl);
        Assert.True(File.Exists(Path.Combine(application.ArtifactsDirectory, "api.log")));
    }

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_repositoryRoot, recursive: true);
        return ValueTask.CompletedTask;
    }
}
