using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateUser;

public static class CreateUserMapping
{
    public static CreateUserResponse ToViewModel(this User user) =>
        new CreateUserResponse(user.Id, user.Username, user.TimeZoneId,
            user.HolidayJurisdiction.Code, user.HolidayJurisdiction.Name, user.IsAdmin, user.Email, user.RequiredOfficePercentage);
}
