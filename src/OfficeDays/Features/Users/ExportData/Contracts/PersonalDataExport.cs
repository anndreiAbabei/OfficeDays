using OfficeDays.Domain;

namespace OfficeDays.Features.Users.ExportData.Contracts;

public sealed record PersonalDataExport
(
    string Format, 
    int FormatVersion,
    DateTimeOffset ExportedAt,
    ExportApplication Application,
    ExportReportContext ReportContext, 
    ExportProfile Profile, 
    ExportPreferences Preferences,
    ExportRequirement[] RequirementHistory, 
    ExportAttendance[] Attendance,
    ExportPlan[] Plans, 
    ExportVacation[] Vacations, 
    ExportHolidayCalendar HolidayCalendar
);

public sealed record ExportApplication(string Name, string Version);
public sealed record ExportReportContext(int CalculationVersion, DateOnly LocalDate, DateOnly RegistrationLocalDate);
public sealed record ExportProfile(Guid Id, 
                                   string Username, 
                                   string NormalizedUsername, 
                                   string? Email, 
                                   string TimeZoneId,
                                   int HolidayJurisdictionId, 
                                   bool IsAdmin, 
                                   DateTimeOffset CreatedAt);
public sealed record ExportPreferences(string? TeamDay);
public sealed record ExportRequirement(Guid Id, Guid UserId, DateOnly StartingFrom, int RequirementValue, DateTimeOffset CreatedAt)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
public sealed record ExportAttendance(Guid Id, Guid UserId, DateOnly Date, bool IsManual, DateTimeOffset CreatedAt);
public sealed record ExportPlan(Guid Id, Guid UserId, DateOnly Date, DateTimeOffset CreatedAt);
public sealed record ExportVacation(Guid Id, Guid UserId, DateOnly From, DateOnly To, DateTimeOffset CreatedAt);
public sealed record ExportHolidayCalendar(int Id, string Code, string Name, ExportHoliday[] Holidays);
public sealed record ExportHoliday(Guid Id, int HolidayJurisdictionId, DateOnly Date, string Name);
