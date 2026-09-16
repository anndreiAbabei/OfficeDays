namespace OfficeDays.Features.BankHolidays;

internal static class BankHolidayValidation
{
    public static (string Key, string Message)? Validate(int year, List<BankHolidayRequest>? request)
    {
        if (year is < 1 or > 9999) return ("year", "Year must be between 1 and 9999.");
        if (request is null) return ("request", "A holiday array is required.");
        if (request.Any(x => x.Date.Year != year)) return ("date", "Every holiday date must belong to the year in the URL.");
        if (request.Any(x => string.IsNullOrWhiteSpace(x.Name) || x.Name.Trim().Length > 150))
            return ("name", "Every holiday needs a name of at most 150 characters.");
        if (request.GroupBy(x => x.Date).Any(group => group.Count() > 1))
            return ("date", "Holiday dates must be unique within the replacement payload.");
        return null;
    }
}
