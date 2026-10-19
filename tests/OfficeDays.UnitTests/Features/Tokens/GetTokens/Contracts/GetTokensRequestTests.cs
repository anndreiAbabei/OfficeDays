using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.UnitTests.Features.Tokens.GetTokens.Contracts;

public sealed class GetTokensRequestTests
{
    [Fact]
    public void Should_CreateRequest()
    {
        // arrange
        // No input is required.

        // act
        var sut = new GetTokensRequest();

        // assert
        Assert.IsAssignableFrom<IRequest>(sut);
    }
}
