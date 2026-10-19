using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Planning.RemovePlan.Contracts;
using OfficeDays.Features.Planning.RemovePlan;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.RemovePlan;

public sealed class RemovePlanEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RemovePlanEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RemovePlanRequest>(epb, executor, "{date}", HttpMethod.Delete);
    }
}
