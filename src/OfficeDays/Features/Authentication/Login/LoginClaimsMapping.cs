using System.Security.Claims;
using OfficeDays.Domain;

namespace OfficeDays.Features.Authentication.Login;

public static class LoginClaimsMapping
{
    public static IEnumerable<Claim> ToClaims(this User user) =>
    [
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("timezone", user.TimeZoneId),
        new Claim("country", user.HolidayJurisdiction.Code),
        new Claim("is_admin", user.IsAdmin ? "true" : "false")
    ];
}
