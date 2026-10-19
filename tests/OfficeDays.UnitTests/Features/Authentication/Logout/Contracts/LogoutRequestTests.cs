using OfficeDays.Features.Authentication.Logout.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Authentication.Logout.Contracts;

public sealed class LogoutRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new LogoutRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
