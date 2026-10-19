using Microsoft.AspNetCore.Builder;
using NSubstitute;
using OfficeDays.Features.Authentication;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Authentication;

public sealed class AuthenticationEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("auth");
        ep.MapEndpoints<IAuthenticationEndpoint>("auth").Returns(group);
        var sut = new AuthenticationEndpointGroup();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IAuthenticationEndpoint>("auth");
    }
}
