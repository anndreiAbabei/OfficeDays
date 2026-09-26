using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OfficeDays.Extensions;

public static class ExceptionEx
{
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 };
}
