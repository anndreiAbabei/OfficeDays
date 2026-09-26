using System.Collections;

namespace OfficeDays.Features.Attendance.GetAttendance.Contracts;

public sealed record GetAttendanceItem(DateOnly Date, DateTimeOffset CreatedAt, bool IsManual);

public sealed record GetAttendanceResponse : IEnumerable<GetAttendanceItem>
{
    private readonly IEnumerable<GetAttendanceItem> _items;
    
    public GetAttendanceResponse(IEnumerable<GetAttendanceItem> items)
    {
        _items = items;
    }
    
    public IEnumerator<GetAttendanceItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
