using NSubstitute;
using OfficeDays.Features.Ui;

namespace OfficeDays.UnitTests.Features.Ui;

public sealed class UiContentTests
{
    [Theory]
    [InlineData("_app/immutable/chunks/app.js", "text/javascript", true)]
    [InlineData("assets/site.css", "text/css", false)]
    [InlineData("data.unknownextension", "application/octet-stream", false)]
    public void Should_LoadIndexAndAssets(string path, string expectedContentType, bool expectedImmutable)
    {
        // arrange
        using var fixture = new UiContentFixture();
        var expectedIndex = fixture.AddFile("index.html", "<html>OfficeDays</html>");
        var expectedBytes = fixture.AddFile(path, "asset content");

        // act
        var sut = new UiContent(fixture.Environment);

        // assert
        Assert.Equal(expectedIndex, sut.Index);
        var asset = Assert.Single(sut.Assets);
        Assert.Equal("/" + path, asset.Path);
        Assert.Equal(expectedBytes, asset.Bytes);
        Assert.Equal(expectedContentType, asset.ContentType);
        Assert.Equal(expectedImmutable, asset.Immutable);
    }

    [Fact]
    public void Should_UseContentRootWhenWebRootIsMissing()
    {
        // arrange
        using var fixture = new UiContentFixture();
        fixture.Environment.WebRootPath.Returns((string)null!);
        var expectedIndex = fixture.AddFile("wwwroot/index.html", "<html>Fallback root</html>");

        // act
        var sut = new UiContent(fixture.Environment);

        // assert
        Assert.Equal(expectedIndex, sut.Index);
        Assert.Empty(sut.Assets);
    }

    [Fact]
    public void Should_RejectMissingIndex()
    {
        // arrange
        using var fixture = new UiContentFixture();

        // act
        var result = Assert.Throws<InvalidOperationException>(() => new UiContent(fixture.Environment));

        // assert
        Assert.Contains("Frontend build missing", result.Message);
    }

    [Fact]
    public void Should_SnapshotContentAtStartup()
    {
        // arrange
        using var fixture = new UiContentFixture();
        var expectedIndex = fixture.AddFile("index.html", "original index");
        var expectedAsset = fixture.AddFile("app.js", "original asset");
        var sut = new UiContent(fixture.Environment);

        // act
        fixture.AddFile("index.html", "changed index");
        fixture.AddFile("app.js", "changed asset");

        // assert
        Assert.Equal(expectedIndex, sut.Index);
        Assert.Equal(expectedAsset, Assert.Single(sut.Assets).Bytes);
    }
}
