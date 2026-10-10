using System.Net;
using System.Text.RegularExpressions;
using OfficeDays.Features.Ui;
using Microsoft.Extensions.DependencyInjection;

namespace OfficeDays.IntegrationTests.Features.Ui;

public sealed class UiIntegrationTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    [InlineData("/dashboard")]
    [InlineData("/login")]
    [InlineData("/register")]
    [InlineData("/profile")]
    [InlineData("/tokens")]
    [InlineData("/admin")]
    public async Task Deep_links_serve_uncached_spa_with_hashed_entry_chunks(string route)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync(route);
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        var matches = Regex.Matches(html, "(?:\\./|/)_app/immutable/[^\"'\\s)]+\\.js");
        Assert.NotEmpty(matches);
        foreach (Match match in matches)
        {
            var asset = await client.GetAsync("/" + match.Value.TrimStart('.', '/'));
            asset.EnsureSuccessStatusCode();
            Assert.Equal("text/javascript", asset.Content.Headers.ContentType!.MediaType);
            Assert.True(asset.Headers.CacheControl!.Public);
            Assert.Equal(TimeSpan.FromDays(365), asset.Headers.CacheControl.MaxAge);
            Assert.Contains("immutable", asset.Headers.CacheControl.ToString());
        }
    }

    [Fact]
    public async Task Every_generated_chunk_is_served_with_correct_caching_and_content_type()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var content = factory.Services.GetRequiredService<UiContent>();
        Assert.Contains(content.Assets, asset => asset.Path.EndsWith(".css"));
        foreach (var asset in content.Assets)
        {
            var response = await client.GetAsync(asset.Path);
            response.EnsureSuccessStatusCode();
            Assert.Equal(asset.Bytes, await response.Content.ReadAsByteArrayAsync());
            Assert.Equal(asset.ContentType, response.Content.Headers.ContentType!.MediaType);
            Assert.Equal(asset.Immutable, response.Headers.CacheControl!.Public);
            Assert.Equal(!asset.Immutable, response.Headers.CacheControl.NoStore);
        }
    }

    [Fact]
    public async Task Public_chunks_are_compressed_but_api_responses_are_not()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var content = factory.Services.GetRequiredService<UiContent>();
        var asset = content.Assets.First(item => item.Path.EndsWith(".js") && item.Bytes.Length > 1000);
        using var request = new HttpRequestMessage(HttpMethod.Get, asset.Path);
        request.Headers.AcceptEncoding.ParseAdd("gzip");
        var response = await client.SendAsync(request);
        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
        Assert.Contains("Accept-Encoding", response.Headers.Vary);
        using var compressed = new System.IO.Compression.GZipStream(
            await response.Content.ReadAsStreamAsync(), System.IO.Compression.CompressionMode.Decompress);
        using var decoded = new MemoryStream();
        await compressed.CopyToAsync(decoded);
        Assert.Equal(asset.Bytes, decoded.ToArray());
        using var apiRequest = new HttpRequestMessage(HttpMethod.Get, "/api/version");
        apiRequest.Headers.AcceptEncoding.ParseAdd("gzip");
        var apiResponse = await client.SendAsync(apiRequest);
        Assert.Empty(apiResponse.Content.Headers.ContentEncoding);
    }

    [Theory]
    [InlineData("/_app/immutable/missing.js")]
    [InlineData("/_app/missing")]
    [InlineData("/assets/missing/app.js")]
    [InlineData("/api/not-real")]
    [InlineData("/health/not-real")]
    public async Task Unknown_api_and_asset_urls_do_not_return_html(string route)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(route)).StatusCode);
    }
}
