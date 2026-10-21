using OfficeDays.Domain;

namespace OfficeDays.Features.Reports.GetReport.Contracts;

public sealed record GetReportMonth(int Month,
                                    int EligibleWorkingDays,
                                    int RequiredOfficePercentage,
                                    int RequiredOfficeDays,
                                    int? OfficeDays,
                                    decimal? AttendancePercentage,
                                    string Result)
{
    public AttendanceRequirementMode Mode { get; init; } = AttendanceRequirementMode.Percentage;
    public int Value { get; init; } = RequiredOfficePercentage;
}
