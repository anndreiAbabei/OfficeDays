using OfficeDays.Features.Reports.GetReport.Contracts;
using OfficeDays.Services;

namespace OfficeDays.Features.Reports.GetReport;

public static class GetReportMapping
{
    public static GetReportResponse ToViewModel(this AttendanceReport report)
    {
        return new GetReportResponse(report.Year, 
                                     report.StartMonth, 
                                     report.EndMonth, 
                                     report.Username, 
                                     report.TimeZoneId, 
                                     report.RegistrationMonth,
                                     report.Today, 
                                     report.GeneratedAt,
                                     new GetReportSummary(report.OfficeDays, 
                                                          report.AttendancePercentage, 
                                                          report.CompletedMonths, 
                                                          report.MonthsMeetingTarget),
                                     report.Months.Select(month => month.ToViewModel()));
    }

    public static GetReportMonth ToViewModel(this AttendanceReportMonth month)
    {
        return new GetReportMonth(month.Month, 
                                  month.EligibleWorkingDays, 
                                  month.RequiredOfficePercentage, 
                                  month.RequiredOfficeDays, 
                                  month.OfficeDays,
                                  month.AttendancePercentage, 
                                  month.State switch
                                  {
                                      ReportMonthState.NotStarted => "notStarted",
                                      ReportMonthState.InProgress => "inProgress",
                                      ReportMonthState.TargetMet => "targetMet",
                                      ReportMonthState.BelowTarget => "belowTarget",
                                      _ => throw new ArgumentOutOfRangeException(nameof(month))
                                  })
        {
            Mode = month.Mode,
            Value = month.Value
        };
    }
}
