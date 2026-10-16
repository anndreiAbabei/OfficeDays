namespace OfficeDays.Features.Reports.GetReport.Contracts;

public sealed record GetReportSummary(int OfficeDays,
                                      decimal? AttendancePercentage,
                                      int CompletedMonths,
                                      int MonthsMeetingTarget);
