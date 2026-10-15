using System.Security.Claims;
using OfficeDays.Extensions;

namespace OfficeDays.UnitTests.Extensions.ClaimsExtensions;

public sealed class ClaimsExtensionsGetUserIdTests
{
    [Fact]
    public void Should_ReturnGuidFromClaimsPrincipal_WhenNameIdentifierExists()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        IEnumerable<Claim> claims = 
        [
            new Claim(ClaimTypes.NameIdentifier, expectedId.ToString())
        ];
        IEnumerable<ClaimsIdentity> identity =
        [
            new ClaimsIdentity(claims)
        ];
        var sut = new ClaimsPrincipal(identity);
        
        // act
        var result = sut.GetUserId();

        // assert
        Assert.Equal(expectedId, result);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_WhenThereIsNoNameIdentifierClaim()
    {
        // arrange
        var sut = new ClaimsPrincipal([new ClaimsIdentity()]);
        const string expectedMessage = "Authenticated principal has no user identifier.";
        
        // act
        Action action = () => sut.GetUserId();

        // assert
        var ex = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(expectedMessage, ex.Message);
    }
}
