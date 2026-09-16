using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions;

internal static class HolidayJurisdictionValidation
{
    public static (string Key, string Message)? Validate(string? code, string? name)
    {
        if (!HolidayJurisdictionCodes.TryNormalize(code, out _))
            return ("code", "A valid country or subdivision code is required, for example RO, GB-NIR, or US.");
        return ValidateName(name);
    }

    public static (string Key, string Message)? ValidateName(string? name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100
            ? ("name", "Name must contain between 1 and 100 characters.")
            : null;
}
