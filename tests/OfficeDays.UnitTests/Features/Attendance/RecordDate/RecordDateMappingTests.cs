using OfficeDays.Domain;
using OfficeDays.Features.Attendance.RecordDate;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate;

public sealed class RecordDateMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        var attendance = new OfficeDays.Domain.Attendance
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsManual = true,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(21)),
            CreatedAt = DateTimeOffset.Now,
            User = new User
            {
                Username = null!,
                NormalizedUsername = null!,
                PasswordHash = null!,
                TimeZoneId = null!
            }
        };

        // act
        var result = attendance.ToViewModel();

        // assert
        Assert.Equal(attendance.Date, result.Date);
        Assert.Equal(attendance.CreatedAt, result.CreatedAt);
        Assert.Equal(attendance.IsManual, result.IsManual);
    }
}
