using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Planning.PlanDate.Contracts;
using OfficeDays.Features.Planning.PlanDate;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Planning.PlanDate;

public sealed class PlanDateEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new PlanDateEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<PlanDateRequest>(epb, executor, "{date}", HttpMethod.Put);
    }
}
