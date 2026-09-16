using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Status;

public static class StatusEndpoints
{
    public static void Map(RouteGroupBuilder api) => api.MapGet("/status", GetStatus).RequireAuthorization();

    private static async Task<IResult> GetStatus(int? year,
                                                 int? month,
                                                 ClaimsPrincipal principal,
                                                 AppDbContext db,
                                                 UserDateService dates)
    {
        var user = await db.Users.FindAsync(principal.GetUserId());
        if (user is null) return Results.NotFound();

        CalculationPeriod period;
        if (!year.HasValue && !month.HasValue)
        {
            var today = dates.Today(user);
            period = CalculationPeriod.ForMonth(today.Year, today.Month);
        }
        else if (!ApiResults.TryMonth(year, month, out period, out var error))
        {
            return error;
        }

        var holidayDates = await db.BankHolidays
            .Where(x => x.HolidayJurisdictionId == user.HolidayJurisdictionId &&
                        x.Date >= period.Start && x.Date <= period.End)
            .Select(x => x.Date)
            .ToListAsync();
        var vacationRows = await db.Vacations
            .Where(x => x.UserId == user.Id && x.From <= period.End && x.To >= period.Start)
            .Select(x => new { x.From, x.To })
            .ToListAsync();
        var officeDates = await db.Attendances
            .Where(x => x.UserId == user.Id && x.Date >= period.Start && x.Date <= period.End)
            .Select(x => x.Date)
            .ToListAsync();
        var result = AttendanceCalculator.Calculate(period, holidayDates,
            vacationRows.Select(x => (x.From, x.To)), officeDates);
        return Results.Ok(result.ToViewModel());
    }
}
