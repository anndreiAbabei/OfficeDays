using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Features.Tokens.GetTokens;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.GetTokens;

public sealed class GetTokensEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new GetTokensEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<GetTokensRequest>(epb, executor, "/", HttpMethod.Get);
    }
}
