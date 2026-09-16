using OfficeDays.Domain;

namespace OfficeDays.Features.Users;

public static class UserMappings
{
    public static UserResponse ToViewModel(this User user) =>
        new UserResponse(user.Id, user.Username, user.TimeZoneId,
            user.HolidayJurisdiction.Code, user.HolidayJurisdiction.Name, user.IsAdmin);
}
