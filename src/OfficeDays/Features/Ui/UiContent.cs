using Microsoft.AspNetCore.StaticFiles;

namespace OfficeDays.Features.Ui;

public sealed record UiAsset(string Path, string ContentType, byte[] Bytes, bool Immutable);

// Snapshot each build at startup. HTML is never cached; hashed chunks are immutable.
public sealed class UiContent
{
    public IReadOnlyList<UiAsset> Assets { get; }
    public byte[] Index { get; }

    public UiContent(IWebHostEnvironment environment)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract -- in unit testing might not work
        var root = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var indexPath = Path.Combine(root, "index.html");
        if (!File.Exists(indexPath))
            throw new InvalidOperationException("Frontend build missing. Run npm ci && npm run build in src/OfficeDays.Web, then dotnet build.");
        Index = File.ReadAllBytes(indexPath);
        var contentTypes = new FileExtensionContentTypeProvider();
        Assets = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(file => file != indexPath)
            .Select(file =>
            {
                var path = "/" + Path.GetRelativePath(root, file).Replace('\\', '/');
                contentTypes.TryGetContentType(file, out var contentType);
                if (path.EndsWith(".js", StringComparison.Ordinal)) contentType = "text/javascript";
                return new UiAsset(path, contentType ?? "application/octet-stream", File.ReadAllBytes(file),
                    path.StartsWith("/_app/immutable/", StringComparison.Ordinal));
            }).ToArray();
    }
}
