using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysHandler(AppDbContext db) : IRequestHandler<GetBankHolidaysRequest>
{
    public async ValueTask<IResult> Handle(GetBankHolidaysRequest input, CancellationToken cancellationToken)
    {
        var countryCode = input.CountryCode;
        var year = input.Year;
        HolidayJurisdictionCodes.TryNormalize(countryCode, out var normalizedCode);

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
}
