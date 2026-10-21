using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public static class GetCurrentUserMapping
{
    public static GetCurrentUserResponse ToViewModel(this User user, UserAttendanceRequirement requirement) =>
        user.ToViewModel(requirement.RequiredOfficePercentage) with
        {
            Mode = requirement.Mode,
            Value = requirement.Value
        };

    public static GetCurrentUserResponse ToViewModel(this User user, int requiredOfficePercentage)
    {
        return new GetCurrentUserResponse(user.Id,
                                          user.Username,
                                          user.TimeZoneId,
                                          user.HolidayJurisdiction.Code,
                                          user.HolidayJurisdiction.Name,
                                          user.IsAdmin,
                                          user.Email,
                                          requiredOfficePercentage,
                                          user.Preferences.TeamDay?.ToString().ToLowerInvariant());
    }
}
