using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public static class GetCurrentUserMapping
{
    public static GetCurrentUserResponse ToViewModel(this User user) => new GetCurrentUserResponse(user.Id, 
                                                                                                   user.Username, 
                                                                                                   user.TimeZoneId,
                                                                                                   user.HolidayJurisdiction.Code, 
                                                                                                   user.HolidayJurisdiction.Name, 
                                                                                                   user.IsAdmin, 
                                                                                                   user.Email, 
                                                                                                   user.RequiredOfficePercentage);
}
