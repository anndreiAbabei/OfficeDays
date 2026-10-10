using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;

namespace OfficeDays.Services;

public sealed class PlanningCalendarReader
{
    private readonly AppDbContext _db;
    private readonly IUserDateService _dates;
    
    public PlanningCalendarReader(AppDbContext db, IUserDateService dates)
    {
        _db = db;
        _dates = dates;
    }
    
    public async Task<IReadOnlyList<PlanningDay>> Read(User user, CalculationPeriod period, CancellationToken ct)
    {
        var plans = await _db.OfficePlans
                            .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
                            .Select(x => x.Date)
                            .ToListAsync(ct);
        
        var attendance = await _db.Attendances
                                 .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
                                 .Select(x => x.Date)
                                 .ToListAsync(ct);
        
        var holidays = await _db.BankHolidays
                               .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId && 
                                           x.Date >= period.Start && 
                                           x.Date <= period.End)
                               .Select(x => new { x.Date, x.Name })
                               .ToListAsync(ct);
        
        var vacations = await _db.Vacations
                                .Where(x => x.UserId == user.Id && x.From <= period.End && x.To >= period.Start)
                                .Select(x => new { x.From, x.To })
                                .ToListAsync(ct);
        
        return PlanningCalculator.Calculate(period, 
                                            _dates.Today(user), 
                                            plans, 
                                            attendance, 
                                            holidays.Select(x => (x.Date, x.Name)), 
                                            vacations.Select(x => (x.From, x.To)));
    }
}
