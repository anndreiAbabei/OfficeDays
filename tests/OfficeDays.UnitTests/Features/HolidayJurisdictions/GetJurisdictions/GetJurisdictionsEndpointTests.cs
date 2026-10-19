using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetJurisdictionsEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetJurisdictionsRequest>(epb, executor, "/", HttpMethod.Get);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }
}
