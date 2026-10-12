using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class VacationTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        var expectedFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
        var expectedTo = DateOnly.FromDateTime(DateTime.Now.AddDays(13));
        var expectedCreatedAt = DateTime.Now;
        var expectedUser = new User
        {
            Username = null!,
            NormalizedUsername = null!,
            PasswordHash = null!,
            TimeZoneId = null!
        };
        
        // act
        var sut = new Vacation
        {
            Id = expectedId,
            UserId = expectedUserId,
            From = expectedFrom,
            To = expectedTo,
            CreatedAt = expectedCreatedAt,
            User = expectedUser,
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedFrom, sut.From);
        Assert.Equal(expectedTo, sut.To);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Same(expectedUser, sut.User);
    }
}
