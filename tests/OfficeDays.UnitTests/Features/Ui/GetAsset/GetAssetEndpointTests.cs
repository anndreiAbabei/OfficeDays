using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OfficeDays.Features.Ui.GetAsset;
using OfficeDays.Features.Ui;

namespace OfficeDays.UnitTests.Features.Ui.GetAsset;

public sealed class GetAssetEndpointTests
{
    [Theory]
    [InlineData("_app/immutable/app.js", "public, max-age=31536000, immutable")]
    [InlineData("app.js", "no-store")]
    public async Task Should_ServeAssetWithExpectedCachePolicy(string path, string expectedCacheControl)
    {
        // arrange
        using var fixture = new UiContentFixture();
        fixture.AddFile("index.html", "index");
        var expectedBytes = fixture.AddFile(path, "console.log(1);");
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var sut = new GetAssetEndpoint(new UiContent(fixture.Environment));
        var context = new DefaultHttpContext { RequestServices = app.Services };
        context.Request.Method = "GET";
        using var body = new MemoryStream();
        context.Response.Body = body;

        // act
        sut.MapEndpoint(app);
        var endpoint = Assert.IsType<RouteEndpoint>(Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)));
        await endpoint.RequestDelegate!(context);

        // assert
        Assert.Equal("/" + path, endpoint.RoutePattern.RawText);
        Assert.Equal(new[] { "GET", "HEAD" }, endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods);
        Assert.Equal(expectedCacheControl, context.Response.Headers.CacheControl);
        Assert.Equal("text/javascript", context.Response.ContentType);
        Assert.Equal(expectedBytes, body.ToArray());
    }

    [Fact]
    public async Task Should_MapNoEndpointsWhenNoAssetsExist()
    {
        // arrange
        using var fixture = new UiContentFixture();
        fixture.AddFile("index.html", "index");
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var sut = new GetAssetEndpoint(new UiContent(fixture.Environment));

        // act
        sut.MapEndpoint(app);

        // assert
        Assert.Empty(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
    }
}
