using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Tokens;

public sealed class TokensEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("tokens");
        ep.MapEndpoints<ITokensEndpoint>("tokens").Returns(group);
        var sut = new TokensEndpointGroup();
        group.MapGet("/probe", () => "ok");

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<ITokensEndpoint>("tokens");
        var endpoint = Assert.Single(((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }
}
