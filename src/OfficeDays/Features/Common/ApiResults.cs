using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Domain;
using OfficeDays.Services;

namespace OfficeDays.Features.Common;

internal static class ApiResults
{
    public static IResult Validation(string message, string key = "request") =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [key] = [message] });

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 };

    public static string NormalizeUsername(string username) => username.Trim().ToUpperInvariant();

    public static bool TryMonth(int? year, int? month, out CalculationPeriod period, out IResult error)
    {
        period = default;
        if (!year.HasValue || !month.HasValue || year is < 1 or > 9999 || month is < 1 or > 12)
        {
            error = Validation("Both year and month must be supplied and valid.");
            return false;
        }

        period = CalculationPeriod.ForMonth(year.Value, month.Value);
        error = Results.Empty;
        return true;
    }
}
