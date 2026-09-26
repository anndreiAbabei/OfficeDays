using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusHandler : IRequestHandler<GetStatusRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _db;
    private readonly IUserDateService _dates;
    
    public GetStatusHandler(ICurrentUser currentUser,
                            AppDbContext db,
                            IUserDateService dates)
    {
        _currentUser = currentUser;
        _db = db;
        _dates = dates;
    }
    
    public async ValueTask<IResult> Handle(GetStatusRequest input, CancellationToken cancellationToken)
    {
        var year = input.Year;
        var month = input.Month;
        
        var user = await _db.Users.FindAsync([_currentUser.Id], cancellationToken);
        
        if (user is null) 
            return Results.NotFound();

        CalculationPeriod period;
        if (!year.HasValue && !month.HasValue)
        {
            var today = _dates.Today(user);
            period = CalculationPeriod.ForMonth(today.Year, today.Month);
        }
        else
            period = CalculationPeriod.ForMonth(year!.Value, month!.Value);

        var holidayDates = await _db.BankHolidays
                                    .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId && x.Date >= period.Start && x.Date <= period.End)
                                    .Select(x => x.Date)
                                    .ToListAsync(cancellationToken);
        
        var vacationRows = await _db.Vacations
                                    .Where(x => x.UserId == user.Id && x.From <= period.End && x.To >= period.Start)
                                    .Select(x => new { x.From, x.To })
                                    .ToListAsync(cancellationToken);
        
        var officeDates = await _db.Attendances
                                   .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
                                   .Select(x => x.Date)
                                   .ToListAsync(cancellationToken);
        
        var result = AttendanceCalculator.Calculate(period, holidayDates, vacationRows.Select(x => (x.From, x.To)), officeDates, user.RequiredOfficePercentage);
        
        return Results.Ok(result.ToViewModel());
    }
}
