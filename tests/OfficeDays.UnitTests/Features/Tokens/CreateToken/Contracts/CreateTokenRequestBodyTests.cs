using OfficeDays.Features.Tokens.CreateToken.Contracts;

namespace OfficeDays.UnitTests.Features.Tokens.CreateToken.Contracts;

public sealed class CreateTokenRequestBodyTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedName = "Name value";
        const string expectedPurpose = "Purpose";

        // act
        var sut = new CreateTokenRequestBody(expectedName, expectedPurpose);

        // assert
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedPurpose, sut.Purpose);
    }
    
    [Fact]
    public void Should_HaveNullPurpose_ByDefault()
    {
        // arrange
        // act
        var sut = new CreateTokenRequestBody("test");

        // assert
        Assert.Null(sut.Purpose);
    }
}
