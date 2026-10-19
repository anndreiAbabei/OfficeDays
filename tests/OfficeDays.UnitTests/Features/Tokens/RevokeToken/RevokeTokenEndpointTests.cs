using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens.RevokeToken.Contracts;
using OfficeDays.Features.Tokens.RevokeToken;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.RevokeToken;

public sealed class RevokeTokenEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RevokeTokenEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RevokeTokenRequest>(epb, executor, "{id:guid}", HttpMethod.Delete);
    }
}
