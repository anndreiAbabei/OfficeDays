using OfficeDays.Features.Attendance.GetAttendance.Contracts;

namespace OfficeDays.Features.Attendance.GetAttendance;

public static class GetAttendanceMapping
{
    public static GetAttendanceResponse ToViewModel(this IEnumerable<Domain.Attendance> attendances)
    {
        return new GetAttendanceResponse(attendances.Select(s => new GetAttendanceItem(s.Date, s.CreatedAt, s.IsManual)));
    }
}
