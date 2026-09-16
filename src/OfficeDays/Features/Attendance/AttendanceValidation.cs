namespace OfficeDays.Features.Attendance;

internal static class AttendanceValidation
{
    public static bool TryParseDate(string value, out DateOnly date) => DateOnly.TryParseExact(value, "yyyy-MM-dd", out date);
}
