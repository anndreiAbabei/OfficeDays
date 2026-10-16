using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Users.UpdateUser;

public static class UpdateUserMapping
{
    public static UpdateUserResponse ToViewModel(this User user, int requiredOfficePercentage) => new UpdateUserResponse(user.Id,
                                                                                           user.Username, 
                                                                                           user.TimeZoneId,
                                                                                           user.HolidayJurisdiction.Code, 
                                                                                           user.HolidayJurisdiction.Name, 
                                                                                           user.IsAdmin, 
                                                                                           user.Email, 
                                                                                           requiredOfficePercentage,
                                                                                           user.Preferences.TeamDay?.ToString().ToLowerInvariant());
}
