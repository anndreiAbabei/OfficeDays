using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Features.Tokens.CreateToken;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken;

public sealed class CreateTokenEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new CreateTokenEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<CreateTokenRequest>(epb, executor, "/", HttpMethod.Post);
    }
}
