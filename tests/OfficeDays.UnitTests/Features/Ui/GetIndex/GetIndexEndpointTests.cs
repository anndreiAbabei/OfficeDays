using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OfficeDays.Features.Ui.GetIndex;
using OfficeDays.Features.Ui;

namespace OfficeDays.UnitTests.Features.Ui.GetIndex;

public sealed class GetIndexEndpointTests
{
    [Fact]
    public async Task Should_CreateIndexAndFallbackEndpoints()
    {
        // arrange
        using var fixture = new UiContentFixture();
        fixture.AddFile("index.html", "index");
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var sut = new GetIndexEndpoint(new UiContent(fixture.Environment));

        // act
        sut.MapEndpoint(app);
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).Cast<RouteEndpoint>().ToArray();

        // assert
        Assert.Equal(2, endpoints.Length);
        Assert.Contains(endpoints, endpoint => endpoint.RoutePattern.RawText == "/index.html");
        Assert.Contains(endpoints, endpoint => endpoint.RoutePattern.RawText == "{*path:nonfile}" && endpoint.Order == int.MaxValue);
        Assert.All(endpoints, endpoint => Assert.Equal(new[] { "GET", "HEAD" }, endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods));
    }

    [Theory]
    [InlineData("/index.html", "/index.html")]
    [InlineData("{*path:nonfile}", "/planning")]
    [InlineData("{*path:nonfile}", "/apiary")]
    public async Task Should_ServeIndexWithoutCaching(string route, string path)
    {
        // arrange
        using var fixture = new UiContentFixture();
        var expectedIndex = fixture.AddFile("index.html", "<html>OfficeDays</html>");
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var sut = new GetIndexEndpoint(new UiContent(fixture.Environment));
        sut.MapEndpoint(app);
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).Cast<RouteEndpoint>().Single(endpoint => endpoint.RoutePattern.RawText == route);
        var context = new DefaultHttpContext { RequestServices = app.Services };
        context.Request.Path = path;
        context.Request.Method = "GET";
        using var body = new MemoryStream();
        context.Response.Body = body;

        // act
        await endpoint.RequestDelegate!(context);

        // assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Equal("text/html; charset=utf-8", context.Response.ContentType);
        Assert.Equal(expectedIndex, body.ToArray());
    }

    [Theory]
    [InlineData("/api")]
    [InlineData("/api/missing")]
    [InlineData("/health/missing")]
    [InlineData("/_app/missing")]
    [InlineData("/assets/missing")]
    public async Task Should_RejectUnknownApiAndAssetPaths(string path)
    {
        // arrange
        using var fixture = new UiContentFixture();
        fixture.AddFile("index.html", "index");
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var sut = new GetIndexEndpoint(new UiContent(fixture.Environment));
        sut.MapEndpoint(app);
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).Cast<RouteEndpoint>().Single(endpoint => endpoint.Order == int.MaxValue);
        var context = new DefaultHttpContext { RequestServices = app.Services };
        context.Request.Path = path;
        using var body = new MemoryStream();
        context.Response.Body = body;

        // act
        await endpoint.RequestDelegate!(context);

        // assert
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Empty(body.ToArray());
    }
}
