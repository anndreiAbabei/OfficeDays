using OfficeDays.Features.Users.CreateUser.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateUser;

public static class CreateUserMapping
{
    public static CreateUserResponse ToViewModel(this User user)
    {
        var requirement = user.AttendanceRequirements.Single();
        
        return new CreateUserResponse
        {
            Id = user.Id,
            Username = user.Username,
            TimeZoneId = user.TimeZoneId,
            CountryCode = user.HolidayJurisdiction.Code,
            CountryName = user.HolidayJurisdiction.Name,
            IsAdmin = user.IsAdmin,
            Email = user.Email,
            RequirementMode = requirement.RequirementMode,
            RequirementValue = requirement.RequirementValue
        };
    }
}
