using OfficeDays.Domain;

namespace OfficeDays.Services;

public enum ReportMonthState
{
    NotStarted,
    InProgress,
    TargetMet,
    BelowTarget
}

public sealed record AttendanceReportMonth(int Month,
                                           int EligibleWorkingDays,
                                           int RequirementValue,
                                           int RequiredOfficeDays,
                                           int? OfficeDays,
                                           decimal? AttendancePercentage,
                                           ReportMonthState State)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}

public sealed record AttendanceReport(int Year,
                                      int StartMonth,
                                      int EndMonth,
                                      string Username,
                                      string TimeZoneId,
                                      DateOnly RegistrationMonth,
                                      DateOnly Today,
                                      DateTimeOffset GeneratedAt,
                                      IReadOnlyList<AttendanceReportMonth> Months,
                                      int OfficeDays,
                                      decimal? AttendancePercentage,
                                      int CompletedMonths,
                                      int MonthsMeetingTarget);
