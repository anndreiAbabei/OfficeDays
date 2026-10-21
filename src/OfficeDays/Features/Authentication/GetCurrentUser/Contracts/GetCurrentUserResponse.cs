using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.GetCurrentUser.Contracts;

public sealed record GetCurrentUserResponse(Guid Id, 
                                            string Username, 
                                            string TimeZoneId, 
                                            string CountryCode, 
                                            string CountryName, 
                                            bool IsAdmin, 
                                            string? Email,
                                            int RequiredOfficePercentage,
                                            string? TeamDay = null)
{
    public AttendanceRequirementMode Mode { get; init; } = AttendanceRequirementMode.Percentage;
    public int Value { get; init; } = RequiredOfficePercentage;
}
