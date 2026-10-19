using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.UpdateRequirement;

public sealed class UpdateRequirementEndpointTests
{
    [Fact]
    public void Should_CreateEndpoint()
    {
        // arrange
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new UpdateRequirementEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<UpdateRequirementRequest>(epb, executor, "me/requirements/{id:guid}", HttpMethod.Put);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }
}
