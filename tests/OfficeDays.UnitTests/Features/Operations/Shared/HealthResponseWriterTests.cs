using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.Shared;

namespace OfficeDays.UnitTests.Features.Operations.Shared;

public sealed class HealthResponseWriterTests
{
    [Theory]
    [InlineData(HealthStatus.Healthy)]
    [InlineData(HealthStatus.Degraded)]
    [InlineData(HealthStatus.Unhealthy)]
    public async Task Should_WriteStatusAndCheckNames(HealthStatus status)
    {
        // arrange
        var expectedStatus = status.ToString();
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;
        var entry = new HealthReportEntry(status, "private database details", TimeSpan.FromSeconds(1), new InvalidOperationException("secret"), new Dictionary<string, object> { ["secret"] = "value" });
        var report = new HealthReport(new Dictionary<string, HealthReportEntry> { ["database"] = entry }, TimeSpan.FromSeconds(1));

        // act
        await HealthResponseWriter.Write(context, report);
        body.Position = 0;
        using var result = await JsonDocument.ParseAsync(body);

        // assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        Assert.Equal(expectedStatus, result.RootElement.GetProperty("status").GetString());
        Assert.Equal(expectedStatus, result.RootElement.GetProperty("checks").GetProperty("database").GetString());
        Assert.Equal(2, result.RootElement.EnumerateObject().Count());
    }

    [Fact]
    public async Task Should_WriteEmptyChecks()
    {
        // arrange
        var context = new DefaultHttpContext();
        using var body = new MemoryStream();
        context.Response.Body = body;
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        // act
        await HealthResponseWriter.Write(context, report);
        body.Position = 0;
        using var result = await JsonDocument.ParseAsync(body);

        // assert
        Assert.Equal("Healthy", result.RootElement.GetProperty("status").GetString());
        Assert.Empty(result.RootElement.GetProperty("checks").EnumerateObject());
    }
}
