using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.Login;

public static class LoginMapping
{
    public static LoginResponse ToViewModel(this User user) =>
        new LoginResponse(user.Id, user.Username, user.TimeZoneId,
            user.HolidayJurisdiction.Code, user.HolidayJurisdiction.Name, user.IsAdmin, user.Email, user.RequiredOfficePercentage);
}
