namespace OfficeDays.Features.Status.GetStatus.Contracts;

public sealed record GetStatusResponse(
    string Period,
    int EligibleWorkingDays,
    int MaximumWfhDays,
    int RequiredOfficeDays,
    int OfficeDays,
    int RemainingOfficeDays,
    decimal ProgressPercentage);
