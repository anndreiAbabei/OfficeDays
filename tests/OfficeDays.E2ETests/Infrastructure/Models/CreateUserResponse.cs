using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace OfficeDays.E2ETests.Infrastructure.Models;

[JsonConverter(typeof(JsonStringEnumConverter<AttendanceRequirementMode>))]
public enum AttendanceRequirementMode
{
    Percentage,
    DaysPerWeek
}

[ExcludeFromCodeCoverage]
public sealed record CreateUserResponse(Guid Id,
                                        string Username,
                                        string TimeZoneId,
                                        string CountryCode,
                                        string CountryName,
                                        bool IsAdmin,
                                        string? Email,
                                        AttendanceRequirementMode RequirementMode,
                                        int RequirementValue);

[ExcludeFromCodeCoverage]
public sealed record CreateUserRequest(string Username,
                                       string Password,
                                       string TimeZoneId,
                                       string CountryCode,
                                       string? Email = null,
                                       AttendanceRequirementMode? RequirementMode = null,
                                       int? RequirementValue = null);