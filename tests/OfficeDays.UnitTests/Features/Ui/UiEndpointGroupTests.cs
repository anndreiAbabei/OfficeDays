using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Ui;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Ui;

public sealed class UiEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("");
        ep.MapEndpoints<IUiEndpoint>("").Returns(group);
        var sut = new UiEndpointGroup();
        group.MapGet("/probe", () => "ok");

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IUiEndpoint>("");
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }
}
