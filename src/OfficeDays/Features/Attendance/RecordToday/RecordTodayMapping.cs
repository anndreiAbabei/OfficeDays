using OfficeDays.Features.Attendance.RecordToday.Contracts;

namespace OfficeDays.Features.Attendance.RecordToday;

public static class RecordTodayMapping
{
    public static RecordTodayResponse ToViewModel(this Domain.Attendance existing)
    {
        return new RecordTodayResponse(existing.Date, existing.CreatedAt, existing.IsManual);
    }
}
