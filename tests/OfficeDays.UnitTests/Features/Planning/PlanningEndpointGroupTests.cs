using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Planning;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Planning;

public sealed class PlanningEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("planning");
        ep.MapEndpoints<IPlanningEndpoint>("planning").Returns(group);
        var sut = new PlanningEndpointGroup();
        group.MapGet("/probe", () => "ok");

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IPlanningEndpoint>("planning");
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }
}
