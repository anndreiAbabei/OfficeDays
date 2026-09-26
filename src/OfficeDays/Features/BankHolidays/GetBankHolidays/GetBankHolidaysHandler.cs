using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.BankHolidays.GetBankHolidays.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.BankHolidays.GetBankHolidays;

public sealed class GetBankHolidaysHandler : IRequestHandler<GetBankHolidaysRequest>
{
    private readonly AppDbContext _db;
    
    public GetBankHolidaysHandler(AppDbContext db)
    {
        _db = db;
    }
    
    public async ValueTask<IResult> Handle(GetBankHolidaysRequest input, CancellationToken cancellationToken)
    {
        var countryCode = input.CountryCode;
        var year = input.Year;
        HolidayJurisdictionCodes.TryNormalize(countryCode, out var normalizedCode);

        var jurisdiction = await _db.HolidayJurisdictions.AsNoTracking()
                                   .SingleOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);
        
        if (jurisdiction is null) 
            return Results.NotFound();

        var start = new DateOnly(year, 1, 1);
        var end = new DateOnly(year, 12, 31);
        var rows = await _db.BankHolidays
                           .AsNoTracking()
                           .Include(x => x.HolidayJurisdiction)
                           .Where(x => x.HolidayJurisdictionId == jurisdiction.Id &&
                                       x.Date >= start && x.Date <= end)
                           .OrderBy(x => x.Date)
                           .ToListAsync(cancellationToken);
        
        return Results.Ok(rows.ToViewModel());
    }
}
