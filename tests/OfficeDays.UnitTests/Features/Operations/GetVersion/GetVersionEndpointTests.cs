using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Features.Operations.GetVersion;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Operations.GetVersion;

public sealed class GetVersionEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetVersionEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetVersionRequest>(epb, executor, "version", HttpMethod.Get);
    }
}
