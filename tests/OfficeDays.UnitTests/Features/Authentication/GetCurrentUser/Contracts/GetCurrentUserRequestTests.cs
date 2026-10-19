using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Authentication.GetCurrentUser.Contracts;

public sealed class GetCurrentUserRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetCurrentUserRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
