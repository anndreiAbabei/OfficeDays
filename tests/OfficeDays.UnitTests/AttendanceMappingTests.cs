using OfficeDays.Domain;
using OfficeDays.Features.Attendance;

namespace OfficeDays.UnitTests;

public sealed class AttendanceMappingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Response_preserves_date_timestamp_and_source(bool isManual)
    {
        var row = new Attendance
        {
            Date = new DateOnly(2026, 9, 16),
            CreatedAt = new DateTimeOffset(2026, 9, 16, 10, 0, 0, TimeSpan.Zero),
            IsManual = isManual
        };
        var response = row.ToViewModel();
        Assert.Equal(row.Date, response.Date);
        Assert.Equal(row.CreatedAt, response.CreatedAt);
        Assert.Equal(isManual, response.IsManual);
    }

    [Fact]
    public void Omitted_source_defaults_to_automatic() => Assert.False(new RecordAttendanceRequest().IsManual);
}
