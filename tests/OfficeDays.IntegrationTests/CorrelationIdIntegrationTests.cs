using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Middleware;

namespace OfficeDays.IntegrationTests;

public sealed class CorrelationIdIntegrationTests
{
    [Theory]
    [InlineData("GET", "/api/version", 200)]
    [InlineData("GET", "/health/live", 200)]
    [InlineData("GET", "/health/ready", 200)]
    [InlineData("GET", "/", 200)]
    [InlineData("GET", "/app.js", 200)]
    [InlineData("GET", "/api/attendance", 401)]
    [InlineData("POST", "/api/auth/login", 400)]
    [InlineData("POST", "/missing-route", 405)]
    public async Task Responses_echo_supplied_id_including_errors(string method, string path, int status)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Add("x-correlation-id", "support-case-123");
        if (path == "/api/auth/login")
            request.Content = new StringContent("{", Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request);

        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("support-case-123", Assert.Single(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName)));
    }

    [Fact]
    public async Task Missing_blank_and_multiple_ids_generate_distinct_ids()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var ids = new HashSet<string>();
        foreach (var supplied in new[] { Array.Empty<string>(), new[] { " " }, new[] { "first", "second" } })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/version");
            if (supplied.Length > 0)
                request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, supplied);
            using var response = await client.SendAsync(request);
            var id = Assert.Single(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName));
            Assert.True(Guid.TryParseExact(id, "N", out _));
            Assert.True(ids.Add(id));
        }
    }

    [Fact]
    public async Task Handled_exceptions_keep_header_and_log_scope()
    {
        var logs = new CapturingLoggerProvider();
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.AddProvider(logs));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRequestHandler<GetVersionRequest>>();
                services.AddScoped<IRequestHandler<GetVersionRequest>, ThrowingVersionHandler>();
            });
        });
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/version");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, "failed-request");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("failed-request", Assert.Single(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName)));
        Assert.Contains(logs.Entries, entry => entry.Level == LogLevel.Error && entry.CorrelationId == "failed-request");
    }

    [Fact]
    public async Task Concurrent_requests_have_isolated_log_scopes()
    {
        var logs = new CapturingLoggerProvider();
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(logs)));
        using var client = factory.CreateClient();
        var ids = Enumerable.Range(0, 8).Select(index => $"parallel-{index}").ToArray();

        await Task.WhenAll(ids.Select(async id =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/version");
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, id);
            using var response = await client.SendAsync(request);
            Assert.Equal(id, Assert.Single(response.Headers.GetValues(CorrelationIdMiddleware.HeaderName)));
        }));

        var completed = logs.Entries.Where(entry => entry.Category == typeof(CorrelationIdMiddleware).FullName).ToArray();
        Assert.Equal(ids.Length, completed.Length);
        Assert.Equal(ids.Order(), completed.Select(entry => entry.CorrelationId).Order());

        factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger("OutsideRequest").LogInformation("Outside request");
        Assert.Null(Assert.Single(logs.Entries, entry => entry.Category == "OutsideRequest").CorrelationId);
    }

    private sealed class ThrowingVersionHandler : IRequestHandler<GetVersionRequest>
    {
        public ValueTask<IResult> Handle(GetVersionRequest request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Deliberate test failure.");
    }

    private sealed record LogEntry(string Category, LogLevel Level, string? CorrelationId);

    private sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
        public ConcurrentQueue<LogEntry> Entries { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this, categoryName);
        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
        public void Dispose() { }

        private sealed class CapturingLogger(CapturingLoggerProvider provider, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => provider._scopes.Push(state);
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var properties = new Dictionary<string, object?>();
                provider._scopes.ForEachScope((scope, values) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object?>> entries)
                        foreach (var entry in entries) values[entry.Key] = entry.Value;
                }, properties);
                provider.Entries.Enqueue(new LogEntry(category, logLevel, properties.GetValueOrDefault("CorrelationId") as string));
            }
        }
    }
}
