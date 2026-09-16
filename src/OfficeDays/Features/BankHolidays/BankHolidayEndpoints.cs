using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Common;
using OfficeDays.Security;

namespace OfficeDays.Features.BankHolidays;

public static class BankHolidayEndpoints
{
    private const string LogCategory = "OfficeDays.Features.BankHolidays";

    public static void Map(RouteGroupBuilder api)
    {
        var holidays = api.MapGroup("/bank-holidays").RequireAuthorization();
        holidays.MapGet("/{year:int}", GetBankHolidays);
        holidays.MapPut("/{year:int}", ReplaceBankHolidays)
            .RequireAuthorization("AdminOnly")
            .AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> GetBankHolidays(int year, AppDbContext db)
    {
        if (year is < 1 or > 9999)
            return ApiResults.Validation("Year must be between 1 and 9999.", "year");
        
        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        var rows = await db.BankHolidays
                           .AsNoTracking()
                           .Where(x => x.Date >= start && x.Date <= end)
                           .OrderBy(x => x.Date)
                           .ToListAsync();
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> ReplaceBankHolidays(int year,
                                                           [FromBody] List<BankHolidayRequest>? request,
                                                           ClaimsPrincipal principal,
                                                           AppDbContext db,
                                                           ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var validation = BankHolidayValidation.Validate(year, request);
        if (validation.HasValue)
            return ApiResults.Validation(validation.Value.Message, validation.Value.Key);
        var replacement = request!;

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.BankHolidays.Where(x => x.Date >= start && x.Date <= end).ExecuteDeleteAsync();
        var rows = replacement.Select(x => x.ToEntity()).ToList();
        db.BankHolidays.AddRange(rows);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogInformation("Admin user {UserId} replaced bank holidays for {Year} with {HolidayCount} entries",
            principal.GetUserId(), year, rows.Count);
        return Results.Ok(rows.OrderBy(x => x.Date).Select(x => x.ToViewModel()));
    }
}
