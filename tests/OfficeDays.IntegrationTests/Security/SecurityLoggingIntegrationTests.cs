using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeDays.Security;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Security;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public sealed class SecurityLoggingIntegrationTests
{
    [Fact]
    public async Task Should_LogSecurityEventsWithoutSecrets_WhenIssuingUsingAndRevokingTokens()
    {
        // Arrange
        const string adminPassword = "VeryStrongAdminPassword!";
        const string unknownToken = "odt_unknown-secret-token";
        const HttpStatusCode expectedRejected = HttpStatusCode.BadRequest;
        const HttpStatusCode expectedCreated = HttpStatusCode.Created;
        const HttpStatusCode expectedDenied = HttpStatusCode.Unauthorized;
        const HttpStatusCode expectedAuthenticated = HttpStatusCode.OK;
        const HttpStatusCode expectedRevoked = HttpStatusCode.NoContent;
        var cancellationToken = TestContext.Current.CancellationToken;
        var logs = new CapturingLoggerProvider();
        await using var original = new OfficeDaysFactory();
        await using var factory = original.WithWebHostBuilder(builder => builder.ConfigureLogging(logging =>
        {
            logging.AddProvider(logs);
            logging.AddFilter("OfficeDays.Security", LogLevel.Debug);
        }));
        using var browser = factory.CreateClient();
        await Login(browser, "Admin", adminPassword);
        using var bearer = factory.CreateClient();

        // Act
        using var rejected = await browser.PostAsJsonAsync("/api/tokens", new { name = "No csrf" }, cancellationToken);

        // Assert a cookie request without its CSRF token.
        Assert.Equal(expectedRejected, rejected.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4101 && entry.Level == LogLevel.Error && entry.Exception is not null);

        var csrf = await Csrf(browser);
        browser.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);
        using var created = await browser.PostAsJsonAsync("/api/tokens", new { name = "Logging check" }, cancellationToken);

        // Assert token creation.
        Assert.Equal(expectedCreated, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var token = Assert.IsType<string>(body.GetProperty("token").GetString());
        var id = body.GetProperty("id").GetGuid();

        // HttpClient normalizes an empty bearer header, so invoke the scheme directly for this branch.
        AuthenticateResult emptyBearerResult;
        using (var scope = factory.Services.CreateScope())
        {
            var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            context.Request.Headers.Authorization = "Bearer ";
            emptyBearerResult = await context.AuthenticateAsync(ApiTokenAuthenticationHandler.SchemeName);
        }

        // Assert an empty bearer header.
        Assert.False(emptyBearerResult.Succeeded);
        Assert.NotNull(emptyBearerResult.Failure);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4002 && entry.Level == LogLevel.Warning);

        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", unknownToken);
        using var unknown = await bearer.GetAsync("/api/auth/me", cancellationToken);

        // Assert an unknown token.
        Assert.Equal(expectedDenied, unknown.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4003 && entry.Level == LogLevel.Warning);

        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var authenticated = await bearer.GetAsync("/api/auth/me", cancellationToken);

        // Assert the issued token.
        Assert.Equal(expectedAuthenticated, authenticated.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4005 && entry.Level == LogLevel.Debug);

        using var revoked = await browser.DeleteAsync($"/api/tokens/{id}", cancellationToken);
        using var denied = await bearer.GetAsync("/api/auth/me", cancellationToken);

        // Assert the revoked token.
        Assert.Equal(expectedRevoked, revoked.StatusCode);
        Assert.Equal(expectedDenied, denied.StatusCode);
        Assert.Contains(logs.Entries, entry => entry.EventId == 4004 && entry.Level == LogLevel.Warning);

        // Assert that no captured message discloses credentials.
        Assert.All(logs.Entries, entry =>
        {
            Assert.DoesNotContain(token, entry.Message);
            Assert.DoesNotContain(unknownToken, entry.Message);
            Assert.DoesNotContain(adminPassword, entry.Message);
        });
    }

    [ExcludeFromCodeCoverage]
    private sealed record LogEntry(int EventId, LogLevel Level, string Message, Exception? Exception);

    [ExcludeFromCodeCoverage]
    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new ConcurrentQueue<LogEntry>();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

        public void Dispose() { }

        private sealed class CapturingLogger(ConcurrentQueue<LogEntry> entries) : ILogger
        {
            private readonly ConcurrentQueue<LogEntry> _entries = entries;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                _entries.Enqueue(new LogEntry(eventId.Id, logLevel, formatter(state, exception), exception));
        }
    }
}
