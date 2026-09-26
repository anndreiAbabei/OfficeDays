using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Status.GetStatus.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Status.GetStatus;

public sealed class GetStatusHandler(ICurrentUser currentUser,
    AppDbContext db,
    IUserDateService dates) : IRequestHandler<GetStatusRequest>
{
    public async ValueTask<IResult> Handle(GetStatusRequest input, CancellationToken cancellationToken)
    {
        var year = input.Year;
        var month = input.Month;
        var user = await db.Users.FindAsync([currentUser.Id], cancellationToken);
        if (user is null) return Results.NotFound();

        CalculationPeriod period;
        if (!year.HasValue && !month.HasValue)
        {
            var today = dates.Today(user);
            period = CalculationPeriod.ForMonth(today.Year, today.Month);
        }
        else
        {
            period = CalculationPeriod.ForMonth(year!.Value, month!.Value);
        }

        var holidayDates = await db.BankHolidays
            .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId &&
                        x.Date >= period.Start && x.Date <= period.End)
            .Select(x => x.Date)
            .ToListAsync(cancellationToken);
        var vacationRows = await db.Vacations
            .Where(x => x.UserId == user.Id && x.From <= period.End && x.To >= period.Start)
            .Select(x => new { x.From, x.To })
            .ToListAsync(cancellationToken);
        var officeDates = await db.Attendances
            .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
            .Select(x => x.Date)
            .ToListAsync(cancellationToken);
        var result = AttendanceCalculator.Calculate(period, holidayDates,
            vacationRows.Select(x => (x.From, x.To)), officeDates, user.RequiredOfficePercentage);
        return Results.Ok(result.ToViewModel());
    }
}
