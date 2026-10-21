using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.Domain;

public sealed class User
{
    public Guid Id { get; init; }
    public required string Username { get; init; }
    public required string NormalizedUsername { get; init; }
    public string? Email { get; set; }
    [SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
    public required string PasswordHash { get; set; }
    public required string TimeZoneId { get; init; }
    public int HolidayJurisdictionId { get; init; }
    public bool IsAdmin { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    public UserPreferences Preferences { get; init; } = new UserPreferences();
    
    public HolidayJurisdiction HolidayJurisdiction { get; init; } = null!;
    public List<Attendance> Attendances { get; init; } = [];
    public List<Vacation> Vacations { get; init; } = [];
    public List<ApiToken> ApiTokens { get; init; } = [];
    public List<UserAttendanceRequirement> AttendanceRequirements { get; init; } = [];
}
