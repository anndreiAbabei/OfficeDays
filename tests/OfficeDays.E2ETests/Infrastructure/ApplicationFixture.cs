using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace OfficeDays.E2ETests.Infrastructure;

public sealed class ApplicationFixture : IAsyncLifetime
{
    public const string AdminPassword = "BrowserTestsOnly!123";
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"officedays-e2e-{Guid.NewGuid():N}");
    private readonly ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();
    private readonly TaskCompletionSource<string> _address = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
    private Process? _process;

    public string BaseUrl { get; private set; } = string.Empty;
    public string RepositoryRoot { get; } = FindRepositoryRoot();
    public string ArtifactsDirectory { get; private set; } = string.Empty;
    public BrowserFixture Browser { get; } = new BrowserFixture();

    public async Task InitializeAsync()
    {
        ArtifactsDirectory = Path.Combine(RepositoryRoot, "artifacts", "e2e", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ArtifactsDirectory);
        Directory.CreateDirectory(_temporaryDirectory);
        try
        {
            var configuration = Environment.GetEnvironmentVariable("E2E_CONFIGURATION") ?? "Debug";
            var appDirectory = Path.Combine(RepositoryRoot, "src", "OfficeDays");
            var assembly = Path.Combine(appDirectory, "bin", configuration, "net10.0", "OfficeDays.dll");
            
            if (!File.Exists(assembly) || !File.Exists(Path.Combine(appDirectory, "wwwroot", "index.html")))
                throw new InvalidOperationException("Build the frontend and backend first. See tests/OfficeDays.E2ETests/README.md.");

            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = appDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add(assembly);
            start.ArgumentList.Add("--urls");
            start.ArgumentList.Add("http://127.0.0.1:0");
            start.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
            start.Environment["DOTNET_ENVIRONMENT"] = "Testing";
            start.Environment["ConnectionStrings__Default"] = $"Data Source={Path.Combine(_temporaryDirectory, "application.db")}";
            start.Environment["DataProtection__KeysPath"] = Path.Combine(_temporaryDirectory, "keys");
            start.Environment["BootstrapAdmin__Password"] = AdminPassword;
            start.Environment["BootstrapAdmin__TimeZoneId"] = "UTC";
            start.Environment["BootstrapAdmin__CountryCode"] = "RO";
            start.Environment["BootstrapAdmin__CountryName"] = "Romania";
            start.Environment["Logging__LogLevel__Default"] = "Warning";
            start.Environment["Logging__LogLevel__Microsoft.Hosting.Lifetime"] = "Information";
            // Many independent users share the runner's IP. Threshold behavior has HTTP integration coverage.
            start.Environment["RateLimiting__Registration__Permits"] = "1000";
            start.Environment["RateLimiting__Login__Permits"] = "1000";
            start.Environment["RateLimiting__LoginAccount__Permits"] = "1000";
            start.Environment["RateLimiting__Source__Permits"] = "10000";
            _process = new Process { StartInfo = start, EnableRaisingEvents = true };
            _process.OutputDataReceived += OnOutput;
            _process.ErrorDataReceived += OnOutput;
            _process.Exited += (_, _) => _address.TrySetException(new InvalidOperationException("API exited before becoming ready. See api.log."));
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            BaseUrl = await _address.Task.WaitAsync(TimeSpan.FromSeconds(60));
            
            // ReSharper disable once ShortLivedHttpClient
            using var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            using var response = await client.GetAsync("/health/ready");
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            await Browser.DisposeAsync();
        }
        finally
        {
            if (_process is not null)
            {
                if (!_process.HasExited)
                    _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
                _process.Dispose();
                _process = null;
            }
            if (Directory.Exists(ArtifactsDirectory))
                await File.WriteAllLinesAsync(Path.Combine(ArtifactsDirectory, "api.log"), _logs);
            if (Directory.Exists(_temporaryDirectory))
                Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private void OnOutput(object sender, DataReceivedEventArgs args)
    {
        if (args.Data is not { } line)
            return;
        _logs.Enqueue(line);
        // OfficeDays uses JSON console logging; inspect the message, not the full JSON line.
        if (line.StartsWith('{'))
        {
            using var document = JsonDocument.Parse(line);
            if (!document.RootElement.TryGetProperty("Message", out var message))
                return;
            line = message.GetString() ?? string.Empty;
        }
        const string marker = "Now listening on: ";
        var index = line.IndexOf(marker, StringComparison.Ordinal);
        if (index >= 0)
            _address.TrySetResult(line[(index + marker.Length)..].Trim());
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OfficeDays.slnx")))
                return directory.FullName;
        }
        throw new DirectoryNotFoundException("Run E2E tests from a checkout of OfficeDays.");
    }
}