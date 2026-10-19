using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Tokens.DeleteToken.Contracts;
using OfficeDays.Features.Tokens.DeleteToken;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Tokens.DeleteToken;

public sealed class DeleteTokenEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new DeleteTokenEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<DeleteTokenRequest>(epb, executor, "{id:guid}/permanent", HttpMethod.Delete);
    }
}
