using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Services;

namespace OfficeDays.Features.Status.GetStatus;

public static class GetStatusMapping
{
    public static GetStatusResponse ToViewModel(this AttendanceStatus status) => new GetStatusResponse(status.Period, status.EligibleWorkingDays, status.MaximumWfhDays, status.RequiredOfficeDays, status.OfficeDays, status.RemainingOfficeDays, status.ProgressPercentage);
}
