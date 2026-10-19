using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Operations;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Operations;

public sealed class HealthEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("health");
        ep.MapEndpoints<IHealthEndpoint>("health").Returns(group);
        var sut = new HealthEndpointGroup();
        group.MapGet("/probe", () => "ok");

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IHealthEndpoint>("health");
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }
}
