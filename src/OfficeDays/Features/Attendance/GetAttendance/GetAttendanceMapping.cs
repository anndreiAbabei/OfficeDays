using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.Features.Attendance.GetAttendance;

public static class GetAttendanceMapping
{
    public static GetAttendanceItem[] ToViewModel(this IEnumerable<Domain.Attendance> attendances)
    {
        return attendances.Select(s => new GetAttendanceItem(s.Date, s.CreatedAt, s.IsManual)).ToArray();
    }
}
