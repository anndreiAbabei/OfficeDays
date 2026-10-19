using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;

namespace OfficeDays.UnitTests.Features.Authentication.GetCsrfToken.Contracts;

public sealed class GetCsrfTokenResponseTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedToken = "Token value";

        // act
        var sut = new GetCsrfTokenResponse(expectedToken);

        // assert
        Assert.Equal(expectedToken, sut.Token);
    }
}
