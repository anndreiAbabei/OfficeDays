using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using OfficeDays.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.OpenTelemetry;

namespace OfficeDays.UnitTests.Configuration;

public sealed class ServiceRegistrationLogExportTests
{
    [Fact]
    public async Task Should_ExportStructuredFieldsAndException_WhenSinkFlushes()
    {
        // Arrange
        const string expectedCorrelationId = "test-office-visit";
        const string expectedCategory = "AttendanceHandler";
        const string expectedEnvironment = "Staging";
        const string expectedVersion = "2.3.0-test";
        var expectedUri = new Uri("http://collector:4318/v1/logs");
        const string expectedMediaType = "application/x-protobuf";
        using var receiver = new RecordingHandler();
        IEnumerable<string> expectedFields =
        [
            "service.name", "OfficeDays", "deployment.environment.name", expectedEnvironment,
            "service.version", expectedVersion, "CorrelationId", expectedCorrelationId,
            "SourceContext", expectedCategory, "UserId", "exception.type", "exception.stacktrace",
            nameof(Should_ExportStructuredFieldsAndException_WhenSinkFlushes)
        ];

        var logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.OpenTelemetry(Configure, ignoreEnvironment: true)
                     .CreateLogger();

        // Act
        await using (logger)
        using (LogContext.PushProperty("CorrelationId", expectedCorrelationId))
        {
            try
            {
                throw new InvalidOperationException("Test exception");
            }
            catch (InvalidOperationException exception)
            {
                logger.ForContext("SourceContext", expectedCategory)
                      .Error(exception, "Failed to record {@Visit}", new { UserId = 42 });
            }
        }

        // Assert
        Assert.Equal(expectedUri, receiver.Uri);
        Assert.Equal(expectedMediaType, receiver.MediaType);
        Assert.NotNull(receiver.Payload);
        // Protobuf string fields are UTF-8. These scope and resource fields are absent from the message body.
        var payload = Encoding.UTF8.GetString(receiver.Payload);
        foreach (var field in expectedFields)
            Assert.Contains(field, payload);
        
        return;

        [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "used before disposing")]
        void Configure(BatchedOpenTelemetrySinkOptions options)
        {
            ServiceRegistration.ConfigureLogExport(options, "http://collector:4318", expectedEnvironment, expectedVersion);
            options.HttpMessageHandler = receiver;
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        public string? MediaType { get; private set; }
        public byte[]? Payload { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri;
            MediaType = request.Content?.Headers.ContentType?.MediaType;
            Payload = request.Content is not null
                          ? await request.Content.ReadAsByteArrayAsync(cancellationToken) 
                          : null;
           
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([])
            };
        }
    }
}
