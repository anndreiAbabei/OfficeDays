using OfficeDays.Domain;
using OfficeDays.Features.Attendance.RecordToday;

namespace OfficeDays.UnitTests.Features.Attendance.RecordToday;

public sealed class RecordTodayMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        const bool expectedIsManual = true;
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedCreatedAt = DateTimeOffset.Now;
        var dto = new OfficeDays.Domain.Attendance
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsManual = expectedIsManual,
            Date = expectedDate,
            CreatedAt = expectedCreatedAt,
            User = new User
            {
                Username = null!,
                NormalizedUsername = null!,
                PasswordHash = null!,
                TimeZoneId = null!
            }
        };

        // act
        var sut = dto.ToViewModel();

        // assert
        Assert.Equal(expectedIsManual, sut.IsManual);
        Assert.Equal(expectedDate, sut.Date);
        Assert.Equal(expectedCreatedAt, sut.CreatedAt);
    }
}
