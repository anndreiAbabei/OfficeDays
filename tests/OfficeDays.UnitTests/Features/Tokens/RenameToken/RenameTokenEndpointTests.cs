using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Features.Tokens.RenameToken;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken;

public sealed class RenameTokenEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new RenameTokenEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<RenameTokenRequest>(epb, executor, "{id:guid}", HttpMethod.Patch);
    }
}
