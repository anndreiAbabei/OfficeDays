using System.Net;

namespace OfficeDays.IntegrationTests.Infrastructure;

public sealed class OpenTelemetryLoggingTests
{
    [Fact]
    public async Task Should_ReturnHealthyResponse_WhenCollectorIsUnreachable()
    {
        // Arrange
        const HttpStatusCode expectedStatus = HttpStatusCode.OK;
        await using var original = new OfficeDaysFactory(useApplicationLogging: true);
        await using var factory = original.WithWebHostBuilder(builder => builder.UseSetting("Serilog:OpenTelemetry:Endpoint",
                                                                                            "http://127.0.0.1:0"));
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }
}
