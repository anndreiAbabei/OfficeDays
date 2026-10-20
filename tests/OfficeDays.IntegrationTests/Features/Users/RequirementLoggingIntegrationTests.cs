using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class RequirementLoggingIntegrationTests
{
    [Fact]
    public async Task Should_LogRejectedRequirementsAndRejectDeletedSessions_WhenRequestsFail()
    {
        var logs = new CapturingLoggerProvider();
        await using var original = new OfficeDaysFactory();
        await using var factory = original.WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddProvider(logs)));
        using var client = factory.CreateClient();
        await CreateUser(client, "requirementlogs");
        await Login(client, "requirementlogs", "Normal-user-password!");
        var csrf = await Csrf(client);
        const string path = "/api/users/me/requirements";
        var history = await client.GetFromJsonAsync<JsonElement>(path);
        var baselineId = history.GetProperty("items")[0].GetProperty("id").GetGuid();
        var body = new { startingFrom = "2027-01-01", requiredOfficePercentage = 60 };
        (await PostJson(client, path, body, csrf)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await PostJson(client, path, body, csrf)).StatusCode);
        var other = await PostJson(client, path, new { startingFrom = "2027-02-01", requiredOfficePercentage = 70 }, csrf);
        var otherId = (await Read(other)).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await PutJson(client, $"{path}/{otherId}", body, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, $"{path}/{baselineId}", body, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Delete(client, $"{path}/{baselineId}", csrf)).StatusCode);
        var missingId = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await PutJson(client, $"{path}/{missingId}", body, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(client, $"{path}/{missingId}", csrf)).StatusCode);

        // Deleted sessions are now rejected before feature handlers run.
        var expectedUnauthorized = HttpStatusCode.Unauthorized;
        await DeleteUser(original, "requirementlogs");
        Assert.Equal(expectedUnauthorized, (await client.GetAsync(path)).StatusCode);
        Assert.Equal(expectedUnauthorized, (await PostJson(client, path, body, csrf)).StatusCode);

        foreach (var eventId in new[] { 4202, 4301, 4302, 4501, 4502, 4503 })
            Assert.Contains(logs.Entries, entry => entry.EventId == eventId && entry.Level == LogLevel.Warning);
        Assert.DoesNotContain(logs.Entries, entry => entry.EventId is 4201 or 4401);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4202 && entry.Exception is not null);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4503 && entry.Exception is not null);
    }

    private sealed record LogEntry(int EventId, LogLevel Level, Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new ConcurrentQueue<LogEntry>();
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);
        public void Dispose() { }

        private sealed class CapturingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                entries.Enqueue(new LogEntry(eventId.Id, logLevel, exception));
        }
    }
}
