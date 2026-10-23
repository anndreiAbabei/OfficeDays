using OfficeDays.Domain;

namespace OfficeDays.Features.Reports.GetReport.Contracts;

public sealed record GetReportMonth(int Month,
                                    int EligibleWorkingDays,
                                    int RequirementValue,
                                    int RequiredOfficeDays,
                                    int? OfficeDays,
                                    decimal? AttendancePercentage,
                                    string Result)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
