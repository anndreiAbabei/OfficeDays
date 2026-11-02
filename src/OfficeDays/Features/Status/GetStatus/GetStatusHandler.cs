using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusHandler : IRequestHandler<GetStatusRequest>
{
    private readonly IAttendanceCalculator _attendanceCalculator;
    private readonly ILogger<GetStatusHandler> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _db;
    private readonly IUserDateService _dates;
    
    public GetStatusHandler(ICurrentUser currentUser,
                            AppDbContext db,
                            IUserDateService dates,
                            ILogger<GetStatusHandler> logger,
                            IAttendanceCalculator attendanceCalculator)
    {
        _attendanceCalculator = attendanceCalculator;
        _logger = logger;
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
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.NotFound();
        }

        var today = _dates.Today(user);
        CalculationPeriod period;
        if (!year.HasValue && !month.HasValue)
        {
            period = CalculationPeriod.ForMonth(today.Year, today.Month);
        }
        else
            period = CalculationPeriod.ForMonth(year!.Value, month!.Value);

        var holidays = await _db.BankHolidays
                                    .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId && x.Date >= period.Start && x.Date <= period.End)
                                    .Select(x => new { x.Date, x.Name })
                                    .ToListAsync(cancellationToken);
        var vacationRows = await _db.Vacations
                                    .Where(x => x.UserId == user.Id && x.From <= period.End && x.To >= period.Start)
                                    .Select(x => new { x.From, x.To })
                                    .ToListAsync(cancellationToken);
        var officeDates = await _db.Attendances
                                   .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
                                   .Select(x => x.Date)
                                   .ToListAsync(cancellationToken);
        var vacations = vacationRows.Select(x => (x.From, x.To));
        var requirement = await _db.UserAttendanceRequirements.AsNoTracking()
                                   .Where(x => x.UserId == user.Id && x.StartingFrom <= period.Start)
                                   .OrderByDescending(x => x.StartingFrom)
                                   .FirstAsync(cancellationToken);
        var result = _attendanceCalculator.Calculate(period, holidays.Select(x => x.Date), vacations, officeDates, requirement.EffectivePercentage);
        var plans = await _db.OfficePlans
                             .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
                             .Select(x => x.Date)
                             .ToListAsync(cancellationToken);
        var days = PlanningCalculator.Calculate(period, today, plans, officeDates, 
                                                holidays.Select(x => (x.Date, x.Name)),
                                                vacationRows.Select(x => (x.From, x.To)));
        var summary = GetStatusSummaryCalculator.Calculate(days, today, result, requirement);
        var response = result.ToViewModel(summary, PlanningCalculator.UpcomingCount(days, today));
        
        return Results.Ok(response);
    }
}
