using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using OfficeDays.Features.Operations.GetVersion;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion;

public sealed class GetVersionHandlerTests
{
    [Fact]
    public async Task Should_UseVersionServiceAndDisableCaching()
    {
        // arrange
        var context = new DefaultHttpContext();
        var appVersion = Substitute.For<IAppVersionService>();
        appVersion.Version.Returns("3.0.0+test-build");
        var sut = new GetVersionHandler(new HttpContextAccessor { HttpContext = context }, appVersion);

        // act
        var result = await sut.Handle(new GetVersionRequest(), CancellationToken.None);

        // assert
        var response = Assert.IsType<Ok<GetVersionResponse>>(result);
        Assert.Equal("3.0.0+test-build", response.Value!.Version);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task Should_ThrowWhenCancelled()
    {
        // arrange
        var sut = new GetVersionHandler(new HttpContextAccessor(), Substitute.For<IAppVersionService>());

        // act & assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Handle(new GetVersionRequest(), new CancellationToken(true)).AsTask());
    }

    [Fact]
    public async Task Should_ThrowWithoutHttpContext()
    {
        // arrange
        var sut = new GetVersionHandler(new HttpContextAccessor(), Substitute.For<IAppVersionService>());

        // act & assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(new GetVersionRequest(), CancellationToken.None).AsTask());
    }
}
