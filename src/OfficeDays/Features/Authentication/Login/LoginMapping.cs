using System.Security.Claims;
using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.Login;

public static class LoginMapping
{
    extension(User user)
    {
        public LoginResponse ToViewModel() =>
            new LoginResponse(user.Id, 
                              user.Username, 
                              user.TimeZoneId,
                              user.HolidayJurisdiction.Code, 
                              user.HolidayJurisdiction.Name, 
                              user.IsAdmin, 
                              user.Email, 
                              user.RequiredOfficePercentage);
        
        public IEnumerable<Claim> ToClaims() =>
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("timezone", user.TimeZoneId),
            new Claim("country", user.HolidayJurisdiction.Code),
            new Claim("is_admin", user.IsAdmin ? "true" : "false")
        ];
    }
}
