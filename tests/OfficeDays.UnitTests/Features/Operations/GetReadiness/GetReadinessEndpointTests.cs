using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OfficeDays.Features.Operations.GetReadiness;

namespace OfficeDays.UnitTests.Features.Operations.GetReadiness;

public sealed class GetReadinessEndpointTests
{
    [Fact]
    public async Task Should_CreateEndpointAndRunChecks()
    {
        // arrange
        var builder = WebApplication.CreateSlimBuilder();
        var checkCalls = 0;
        builder.Services.AddHealthChecks().AddCheck("database", () =>
        {
            checkCalls++;
            return HealthCheckResult.Unhealthy("Database unavailable");
        });
        await using var app = builder.Build();
        var sut = new GetReadinessEndpoint();
        var context = new DefaultHttpContext { RequestServices = app.Services };
        using var body = new MemoryStream();
        context.Response.Body = body;

        // act
        sut.MapEndpoint(app);
        var endpoint = Assert.IsType<RouteEndpoint>(Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)));
        await endpoint.RequestDelegate!(context);
        body.Position = 0;
        using var result = await JsonDocument.ParseAsync(body);

        // assert
        Assert.Equal("ready", endpoint.RoutePattern.RawText);
        Assert.Equal(1, checkCalls);
        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("Unhealthy", result.RootElement.GetProperty("status").GetString());
        Assert.Equal("Unhealthy", result.RootElement.GetProperty("checks").GetProperty("database").GetString());
    }
}
