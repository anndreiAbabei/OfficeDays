using Microsoft.AspNetCore.Builder;
using NSubstitute;
using OfficeDays.Features.Users;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Users;

public sealed class UsersEndpointGroupTests
{
    [Fact]
    public async Task Should_MapGroup()
    {
        // arrange
        var ep = Substitute.For<IEndpointGroupBuilder>();
        await using var app = WebApplication.CreateSlimBuilder().Build();
        var group = app.MapGroup("users");
        ep.MapEndpoints<IUsersEndpoint>("users").Returns(group);
        var sut = new UsersEndpointGroup();

        // act
        sut.MapGroup(ep);

        // assert
        ep.Received(1).MapEndpoints<IUsersEndpoint>("users");
    }
}
