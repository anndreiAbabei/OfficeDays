using System.ComponentModel.DataAnnotations;
using OfficeDays.Services;

namespace OfficeDays.Features.Users;

internal static class UserValidation
{
    public static (string Key, string Message)? Validate(CreateUserRequest request)
    {
        var username = request.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Length is < 3 or > 100)
            return ("username", "Username must contain between 3 and 100 characters.");
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 12)
            return ("password", "Password must contain at least 12 characters.");
        if (string.IsNullOrWhiteSpace(request.TimeZoneId) || !UserDateService.IsValidTimeZone(request.TimeZoneId))
            return ("timeZoneId", "A valid IANA timezone is required, for example Europe/Bucharest.");
        if (!HolidayJurisdictionCodes.TryNormalize(request.CountryCode, out _))
            return ("countryCode", "A valid country or subdivision code is required, for example RO, GB-NIR, or US.");
        return ValidateEmail(request.Email);
    }
    
    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim();

    public static (string Key, string Message)? ValidateEmail(string? email)
    {
        var normalized = NormalizeEmail(email);
        
        const int maxEmailLen = 250;

        var emailValidator = new EmailAddressAttribute();
        if (normalized is null || normalized.Length > maxEmailLen || !emailValidator.IsValid(normalized))
            return ("email", $"Enter a valid email address of at most {maxEmailLen} characters.");
        
        return null;
    }
}
