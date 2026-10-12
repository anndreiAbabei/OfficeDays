using OfficeDays.Domain;

namespace OfficeDays.UnitTests.Domain;

public sealed class AttendanceTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        var expectedId = Guid.NewGuid();
        var expectedUserId = Guid.NewGuid();
        const bool expectedIsManual = true;
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedCreatedAt = DateTime.Now;
        var expectedUser = new User
        {
            Username = null!,
            NormalizedUsername = null!,
            PasswordHash = null!,
            TimeZoneId = null!
        };
        
        // act
        var sut = new Attendance
        {
            Id = expectedId,
            UserId = expectedUserId,
            IsManual = expectedIsManual,
            Date = expectedDate,
            CreatedAt = expectedCreatedAt,
            User = expectedUser
        };

        // assert
        Assert.Equal(expectedId, sut.Id);
        Assert.Equal(expectedUserId, sut.UserId);
        Assert.Equal(expectedIsManual, sut.IsManual);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
        Assert.Same(expectedUser, sut.User);
    }
}
