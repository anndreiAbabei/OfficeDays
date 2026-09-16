using System.Globalization;

namespace OfficeDays.Services;

public static class HolidayJurisdictionCodes
{
    public static bool TryNormalize(string? value, out string code)
    {
        code = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (code == "UK") 
            code = "GB";
        
        if (code.StartsWith("UK-", StringComparison.Ordinal)) code = $"GB-{code[3..]}";

        var parts = code.Split('-');
        
        if (parts.Length is < 1 or > 2 || parts[0].Length != 2 ||
            parts[0].Any(character => character is < 'A' or > 'Z'))
        {
            return false;
        }

        if (parts.Length == 2 && (parts[1].Length is < 1 or > 3 || parts[1].Any(character => !char.IsAsciiLetterOrDigit(character))))
            return false;

        try
        {
            var region = new RegionInfo(parts[0]);
            return string.Equals(region.TwoLetterISORegionName, parts[0], StringComparison.Ordinal);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
