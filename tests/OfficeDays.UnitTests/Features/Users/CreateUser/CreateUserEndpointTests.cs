using OfficeDays.Security.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Features.Users.CreateUser;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Users.CreateUser;

public sealed class CreateUserEndpointTests
{
    [Fact]
    public void Should_ProtectSensitiveEndpoint_WhenMapped()
    {
        // arrange
        var expectedLimit = new SensitiveRequestLimit("registration");
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new CreateUserEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<CreateUserRequest>(epb, executor, "/", HttpMethod.Post);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Equal(expectedLimit, endpoint.Metadata.GetMetadata<SensitiveRequestLimit>());
    }
}
