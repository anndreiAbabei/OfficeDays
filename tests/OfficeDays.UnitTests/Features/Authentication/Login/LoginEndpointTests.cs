using OfficeDays.Security.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Features.Authentication.Login;
using OfficeDays.Infrastructure;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Authentication.Login;

public sealed class LoginEndpointTests
{
    [Fact]
    public void Should_ProtectSensitiveEndpoint_WhenMapped()
    {
        // arrange
        var expectedLimit = new SensitiveRequestLimit("login", "username");
        var executor = new RequestExecutor();
        var epb = Substitute.For<IEndpointRouteBuilder>();
        epb.DataSources.Returns([]);
        var sut = new LoginEndpoint(executor);

        // act
        sut.MapEndpoint(epb);

        // assert
        Assert.Endpoint<LoginRequest>(epb, executor, "login", HttpMethod.Post);
        var endpoint = Assert.Single(epb.DataSources.SelectMany(source => source.Endpoints));
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Equal(expectedLimit, endpoint.Metadata.GetMetadata<SensitiveRequestLimit>());
    }
}
