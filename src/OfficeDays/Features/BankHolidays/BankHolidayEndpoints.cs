using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.BankHolidays;

public static class BankHolidayEndpoints
{
    private const string LogCategory = "OfficeDays.Features.BankHolidays";

    public static void Map(RouteGroupBuilder api)
    {
        var holidays = api.MapGroup("/bank-holidays");
        holidays.MapGet("/{countryCode}/{year:int}", GetBankHolidays).AllowAnonymous();
        holidays.MapPut("/{countryCode}/{year:int}", ReplaceBankHolidays)
            .RequireAuthorization("AdminOnly")
            .AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> GetBankHolidays(string countryCode,
                                                       int year,
                                                       AppDbContext db,
                                                       CancellationToken cancellationToken)
    {
        if (!HolidayJurisdictionCodes.TryNormalize(countryCode, out var normalizedCode))
            return ApiResults.Validation("A valid country or subdivision code is required.", "countryCode");
        if (year is < 1 or > 9999)
            return ApiResults.Validation("Year must be between 1 and 9999.", "year");

        var jurisdiction = await db.HolidayJurisdictions.AsNoTracking()
                                   .SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
        if (jurisdiction is null) return Results.NotFound();

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        var rows = await db.BankHolidays
                           .AsNoTracking()
                           .Include(x => x.HolidayJurisdiction)
                           .Where(x => x.HolidayJurisdictionId == jurisdiction.Id &&
                                       x.Date >= start && x.Date <= end)
                           .OrderBy(x => x.Date)
                           .ToListAsync(cancellationToken);
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> ReplaceBankHolidays(string countryCode,
                                                           int year,
                                                           [FromBody] List<BankHolidayRequest>? request,
                                                           ClaimsPrincipal principal,
                                                           AppDbContext db,
                                                           ILoggerFactory loggerFactory,
                                                           CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        if (!HolidayJurisdictionCodes.TryNormalize(countryCode, out var normalizedCode))
            return ApiResults.Validation("A valid country or subdivision code is required.", "countryCode");
        var validation = BankHolidayValidation.Validate(year, request);
        if (validation.HasValue)
            return ApiResults.Validation(validation.Value.Message, validation.Value.Key);
        var replacement = request!;

        var jurisdiction = await db.HolidayJurisdictions.SingleOrDefaultAsync(
            x => x.Code == normalizedCode, cancellationToken);
        if (jurisdiction is null) return Results.NotFound();

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.BankHolidays
                .Where(x => x.HolidayJurisdictionId == jurisdiction.Id &&
                            x.Date >= start && x.Date <= end)
                .ExecuteDeleteAsync(cancellationToken);
        var rows = replacement.Select(x => x.ToEntity(jurisdiction)).ToList();
        db.BankHolidays.AddRange(rows);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Admin user {UserId} replaced bank holidays for {JurisdictionCode} in {Year} with {HolidayCount} entries",
            principal.GetUserId(), jurisdiction.Code, year, rows.Count);
        return Results.Ok(rows.OrderBy(x => x.Date).Select(x => x.ToViewModel()));
    }
}
