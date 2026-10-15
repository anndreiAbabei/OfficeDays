using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class ApiTokenTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var expectedName = Guid.NewGuid().ToString();
        var expectedTokenHash = Guid.NewGuid().ToString();
        var expectedCreatedAt = DateTime.Now.AddHours(-1);
        var expectedLastUsedAt = DateTime.Now.AddSeconds(-12);
        var expectedRevokedAt = DateTime.Now;
        var expectedUser = new User
        {
            Username = null!,
            NormalizedUsername = null!,
            PasswordHash = null!,
            TimeZoneId = null!
        };
        
        // act
        var sut = new ApiToken
        {
            Id = expectedId,
            UserId = expectedUserId,
            Name = expectedName,
            TokenHash = expectedTokenHash,
            CreatedAt = expectedCreatedAt,
            LastUsedAt = expectedLastUsedAt,
            RevokedAt = expectedRevokedAt,
            User = expectedUser
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedTokenHash, sut.TokenHash);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Equal(expectedLastUsedAt, sut.LastUsedAt);
        Assert.Equal(expectedRevokedAt, sut.RevokedAt);
        Assert.Same(expectedUser, sut.User);
    }
}
