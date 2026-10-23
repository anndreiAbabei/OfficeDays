using OfficeDays.Domain;

namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserResponse(Guid Id, 
                                        string Username, 
                                        string TimeZoneId, 
                                        string CountryCode, 
                                        string CountryName, 
                                        bool IsAdmin, 
                                        string? Email, 
                                        int RequirementValue,
                                        string? TeamDay = null)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
