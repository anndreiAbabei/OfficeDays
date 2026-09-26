using OfficeDays.Features.Attendance.RecordDate.Contracts;

namespace OfficeDays.Features.Attendance.RecordDate;

public static class RecordDateMapping
{
    public static RecordDateResponse ToViewModel(this Domain.Attendance existing)
    {
        return new RecordDateResponse(existing.Date, existing.CreatedAt, existing.IsManual);
    }
}
