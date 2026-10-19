using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Features.Status.GetStatus;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Status.GetStatus;

public sealed class GetStatusEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetStatusEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetStatusRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
