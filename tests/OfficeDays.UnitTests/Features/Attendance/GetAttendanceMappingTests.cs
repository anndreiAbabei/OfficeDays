using OfficeDays.Features.Attendance.GetAttendance;

namespace OfficeDays.UnitTests.Features.Attendance;

public sealed class GetAttendanceMappingTests
{
    [Fact]
    public void Should_MapToViewModel()
    {
        // arrange
        List<OfficeDays.Domain.Attendance> attendances =
        [
            new OfficeDays.Domain.Attendance
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                IsManual = true,
                Date = DateOnly.FromDateTime(DateTime.Now),
                CreatedAt = DateTime.Now,
                User = null!
            }
        ];

        // act
        var result = attendances.ToViewModel();

        // assert
        Assert.All(attendances.Zip(result), t =>
        {
            var (source, destination) = t;
            Assert.Equal(source.Date, destination.Date);
            Assert.Equal(source.CreatedAt, destination.CreatedAt);
            Assert.Equal(source.IsManual, destination.IsManual);
        });
    }
}
