using System.Security.Cryptography;
using System.Text;

namespace OfficeDays.Endpoints;

public static class UiEndpointMappings
{
    public static void MapUiEndpoints(this WebApplication app)
    {
        var files = app.Environment.WebRootFileProvider;
        using var reader = new StreamReader(files.GetFileInfo("index.html").CreateReadStream());
        var html = reader.ReadToEnd();

        foreach (var (name, contentType) in new[]
        {
            ("app.js", "text/javascript; charset=utf-8"),
            ("styles.css", "text/css; charset=utf-8")
        })
        {
            using var stream = files.GetFileInfo(name).CreateReadStream();
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var bytes = buffer.ToArray();
            var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
            var path = $"/assets/{hash}/{name}";
            html = html.Replace($"\"/{name}\"", $"\"{path}\"", StringComparison.Ordinal);

            // Snapshot both the URL and bytes so a URL can never serve different content.
            app.MapGet(path, (HttpContext context) =>
            {
                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                return Results.Bytes(bytes, contentType);
            }).AllowAnonymous();
            app.MapGet($"/{name}", (HttpContext context) =>
            {
                context.Response.Headers.CacheControl = "no-store";
                return Results.Bytes(bytes, contentType);
            }).AllowAnonymous();
        }

        var page = Encoding.UTF8.GetBytes(html);
        IResult Index(HttpContext context)
        {
            // HTML must always discover the current content-addressed asset URLs.
            context.Response.Headers.CacheControl = "no-store";
            return Results.Bytes(page, "text/html; charset=utf-8");
        }

        app.MapGet("/index.html", Index).AllowAnonymous();
        app.MapFallback(Index)
            .WithMetadata(new HttpMethodMetadata(["GET", "HEAD"]))
            .AllowAnonymous();
    }
}
