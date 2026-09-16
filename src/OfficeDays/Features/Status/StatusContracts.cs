namespace OfficeDays.Features.Status;

public sealed record StatusResponse(
    string Period,
    int EligibleWorkingDays,
    int MaximumWfhDays,
    int RequiredOfficeDays,
    int OfficeDays,
    int RemainingOfficeDays,
    decimal ProgressPercentage);
