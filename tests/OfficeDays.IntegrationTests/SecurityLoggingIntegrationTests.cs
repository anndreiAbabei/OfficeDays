using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeDays.Security;

namespace OfficeDays.IntegrationTests;

public sealed class SecurityLoggingIntegrationTests
{
    [Fact]
    public async Task Token_lifecycle_and_antiforgery_failures_emit_safe_generated_logs()
    {
        var logs = new CapturingLoggerProvider();
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureLogging(logging =>
        {
            logging.AddProvider(logs);
            logging.AddFilter("OfficeDays.Security", LogLevel.Debug);
        }));
        using var browser = factory.CreateClient();
        using var login = await browser.PostAsJsonAsync("/api/auth/login", new
        {
            username = "Admin", password = "VeryStrongAdminPassword!"
        });
        login.EnsureSuccessStatusCode();

        using var rejected = await browser.PostAsJsonAsync("/api/tokens", new { name = "No csrf" });
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4101 && entry.Level == LogLevel.Error && entry.Exception is not null);

        var csrf = (await browser.GetFromJsonAsync<JsonElement>("/api/auth/csrf")).GetProperty("token").GetString();
        browser.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);
        using var created = await browser.PostAsJsonAsync("/api/tokens", new { name = "Logging check" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString()!;
        var id = body.GetProperty("id").GetGuid();

        using var bearer = factory.CreateClient();
        // HttpClient normalizes an empty bearer header, so invoke the scheme directly for this branch.
        using (var scope = factory.Services.CreateScope())
        {
            var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            context.Request.Headers.Authorization = "Bearer ";
            var result = await context.AuthenticateAsync(ApiTokenAuthenticationHandler.SchemeName);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Failure);
        }
        Assert.Contains(logs.Entries, entry => entry.EventId == 4002 && entry.Level == LogLevel.Warning);

        const string unknownToken = "odt_unknown-secret-token";
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", unknownToken);
        using var unknown = await bearer.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4003 && entry.Level == LogLevel.Warning);

        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var authenticated = await bearer.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4005 && entry.Level == LogLevel.Debug);

        using var revoked = await browser.DeleteAsync($"/api/tokens/{id}");
        Assert.Equal(HttpStatusCode.NoContent, revoked.StatusCode);
        using var denied = await bearer.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4004 && entry.Level == LogLevel.Warning);
        Assert.All(logs.Entries, entry =>
        {
            Assert.DoesNotContain(token, entry.Message);
            Assert.DoesNotContain(unknownToken, entry.Message);
            Assert.DoesNotContain("VeryStrongAdminPassword!", entry.Message);
        });
    }

    private sealed record LogEntry(int EventId, LogLevel Level, string Message, Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);
        public void Dispose() { }

        private sealed class CapturingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                entries.Enqueue(new LogEntry(eventId.Id, logLevel, formatter(state, exception), exception));
        }
    }
}
