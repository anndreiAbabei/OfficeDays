using OfficeDays.Features.Ui;

namespace OfficeDays.UnitTests.Features.Ui;

public sealed class UiAssetTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedPath = "/app.js";
        var expectedContentType = "text/javascript";
        byte[] expectedBytes = [1, 2, 3];
        var expectedImmutable = true;

        // act
        var sut = new UiAsset(expectedPath, expectedContentType, expectedBytes, expectedImmutable);

        // assert
        Assert.Equal(expectedPath, sut.Path);
        Assert.Equal(expectedContentType, sut.ContentType);
        Assert.Same(expectedBytes, sut.Bytes);
        Assert.Equal(expectedImmutable, sut.Immutable);
    }
}
