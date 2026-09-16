using OfficeDays.Services;

namespace OfficeDays.Features.Status;

public static class StatusMappings
{
    public static StatusResponse ToViewModel(this AttendanceStatus status) => new StatusResponse(status.Period, status.EligibleWorkingDays, status.MaximumWfhDays, status.RequiredOfficeDays, status.OfficeDays, status.RemainingOfficeDays, status.ProgressPercentage);
}
