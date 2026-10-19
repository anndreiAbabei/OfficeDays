using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Planning.GetPlanning.Contracts;
using OfficeDays.Features.Planning.GetPlanning;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.GetPlanning;

public sealed class GetPlanningEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetPlanningEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetPlanningRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
