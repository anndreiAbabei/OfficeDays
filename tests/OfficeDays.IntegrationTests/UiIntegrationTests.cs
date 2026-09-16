using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace OfficeDays.IntegrationTests;

public sealed class UiIntegrationTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    [InlineData("/dashboard")]
    public async Task Html_is_not_cached_and_references_content_addressed_assets(string route)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync(route);
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("src=\"/app.js\"", html);
        Assert.DoesNotContain("href=\"/styles.css\"", html);

        foreach (var (name, type) in new[] { ("app.js", "text/javascript"), ("styles.css", "text/css") })
        {
            var match = Regex.Match(html, $"/assets/([a-f0-9]{{64}})/{Regex.Escape(name)}");
            Assert.True(match.Success);
            var asset = await client.GetAsync(match.Value);
            asset.EnsureSuccessStatusCode();
            Assert.Equal(type, asset.Content.Headers.ContentType!.MediaType);
            var bytes = await asset.Content.ReadAsByteArrayAsync();
            Assert.Equal(match.Groups[1].Value, Convert.ToHexStringLower(SHA256.HashData(bytes)));
            Assert.True(asset.Headers.CacheControl!.Public);
            Assert.Equal(TimeSpan.FromDays(365), asset.Headers.CacheControl.MaxAge);
            Assert.Contains("immutable", asset.Headers.CacheControl.ToString());
            Assert.Equal(bytes, await client.GetByteArrayAsync(match.Value));

            var legacy = await client.GetAsync($"/{name}");
            Assert.True(legacy.Headers.CacheControl!.NoStore);
            Assert.Equal(bytes, await legacy.Content.ReadAsByteArrayAsync());
        }
    }

    [Fact]
    public async Task Unknown_asset_hash_returns_not_found_instead_of_html_or_new_asset()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/assets/{new string('0', 64)}/app.js")).StatusCode);
    }
}
