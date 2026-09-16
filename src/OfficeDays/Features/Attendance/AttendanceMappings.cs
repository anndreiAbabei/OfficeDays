namespace OfficeDays.Features.Attendance;

public static class AttendanceMappings
{
    public static AttendanceResponse ToViewModel(this Domain.Attendance attendance) => new AttendanceResponse(attendance.Date, attendance.CreatedAt, attendance.IsManual);
}
