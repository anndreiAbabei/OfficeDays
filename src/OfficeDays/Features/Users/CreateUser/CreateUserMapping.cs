using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateUser;

public static class CreateUserMapping
{
    public static CreateUserResponse ToViewModel(this User user) => new CreateUserResponse
    {
        Id = user.Id,
        Username = user.Username,
        TimeZoneId = user.TimeZoneId,
        CountryCode = user.HolidayJurisdiction.Code,
        CountryName = user.HolidayJurisdiction.Name,
        IsAdmin = user.IsAdmin,
        Email = user.Email,
        RequiredOfficePercentage = user.RequiredOfficePercentage
    };
}
