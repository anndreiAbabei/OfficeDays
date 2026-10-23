using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public static class GetCurrentUserMapping
{
    public static GetCurrentUserResponse ToViewModel(this User user, UserAttendanceRequirement requirement)
    {
        return new GetCurrentUserResponse(user.Id,
                                          user.Username,
                                          user.TimeZoneId,
                                          user.HolidayJurisdiction.Code,
                                          user.HolidayJurisdiction.Name,
                                          user.IsAdmin,
                                          user.Email,
                                          requirement.RequirementValue,
                                          user.Preferences.TeamDay?.ToString().ToLowerInvariant())
        {
            RequirementMode = requirement.RequirementMode
        };
    }
}
