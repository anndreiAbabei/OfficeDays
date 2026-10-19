using OfficeDays.Features.Tokens.RenameToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken.Contracts;

public sealed class RenameTokenRequestTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedBody = new RenameTokenRequestBody("Name value");

        // act
        var sut = new RenameTokenRequest
        {
            Id = expectedId,
            Body = expectedBody
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedBody, sut.Body);
    }
}
