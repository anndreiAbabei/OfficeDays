using System.Security.Cryptography;
using System.Text;

namespace OfficeDays.Features.Ui;

public sealed record UiAsset(string Name, string Path, string ContentType, byte[] Bytes);

public sealed class UiContent
{
    public IReadOnlyList<UiAsset> Assets { get; }
    public byte[] Index { get; }

    public UiContent(IWebHostEnvironment environment)
    {
        var files = environment.WebRootFileProvider;
        using var reader = new StreamReader(files.GetFileInfo("index.html").CreateReadStream());
        var html = reader.ReadToEnd();
        var assets = new List<UiAsset>();
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
            assets.Add(new UiAsset(name, path, contentType, bytes));
        }
        Assets = assets;
        Index = Encoding.UTF8.GetBytes(html);
    }
}
