using OfficeDays.Features.Tokens.RenameToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.RenameToken.Contracts;

public sealed class RenameTokenRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedName = "Name value";

        // act
        var sut = new RenameTokenRequestBody(expectedName);

        // assert
        Assert.Equal(expectedName, sut.Name);
    }
}
