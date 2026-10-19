using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Authentication.GetCsrfToken.Contracts;

public sealed class GetCsrfTokenRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetCsrfTokenRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
