using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new DeleteJurisdictionEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<DeleteJurisdictionRequest>(epb, executor, "{code}", HttpMethod.Delete);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.Equal("AdminOnly", Assert.Single(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()).Policy);
    }
}
