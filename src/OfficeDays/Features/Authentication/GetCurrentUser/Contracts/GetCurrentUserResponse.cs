using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.GetCurrentUser.Contracts;

public sealed record GetCurrentUserResponse(Guid Id,
                                            string Username,
                                            string TimeZoneId,
                                            string CountryCode,
                                            string CountryName,
                                            bool IsAdmin,
                                            string? Email,
                                            int RequirementValue,
                                            string? TeamDay = null)
{
    public Appearance Appearance { get; init; } = Appearance.System;

    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
