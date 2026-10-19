using System.Text;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace OfficeDays.UnitTests.Features.Ui;

internal sealed class UiContentFixture : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "OfficeDays.UnitTests", Guid.NewGuid().ToString("N"));
    public IWebHostEnvironment Environment { get; } = Substitute.For<IWebHostEnvironment>();

    public UiContentFixture()
    {
        Directory.CreateDirectory(Root);
        Environment.WebRootPath.Returns(Root);
        Environment.ContentRootPath.Returns(Root);
    }

    public byte[] AddFile(string path, string content)
    {
        var file = Path.Combine(Root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var bytes = Encoding.UTF8.GetBytes(content);
        File.WriteAllBytes(file, bytes);
        return bytes;
    }

    public void Dispose() => Directory.Delete(Root, true);
}
