using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Automation;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Automation;

public sealed class AutomationEndpointGroupTests
{
    [Fact]
    public async Task Should_RequireAuthorization_WhenMappingAutomationEndpoints()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("automation");
        ep.MapEndpoints<IAutomationEndpoint>("automation").Returns(group);
        var sut = new AutomationEndpointGroup();
        group.MapGet("/probe", () => "ok");

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IAutomationEndpoint>("automation");
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }
}
