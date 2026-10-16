namespace OfficeDays.Features.Reports.GetReport.Contracts;

public sealed record GetReportResponse(int Year,
                                       int StartMonth,
                                       int EndMonth,
                                       string Username,
                                       string TimeZoneId,
                                       DateOnly RegistrationMonth,
                                       DateOnly Today,
                                       DateTimeOffset GeneratedAt,
                                       GetReportSummary Summary,
                                       IEnumerable<GetReportMonth> Months);
