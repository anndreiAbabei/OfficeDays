using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Vacations.GetVacations.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Vacations.GetVacations;

public sealed class GetVacationsHandler(ICurrentUser currentUser,
    AppDbContext db) : IRequestHandler<GetVacationsRequest>
{
    public async ValueTask<IResult> Handle(GetVacationsRequest input, CancellationToken cancellationToken)
    {
        var year = input.Year;
        var month = input.Month;
        var query = db.Vacations
                      .AsNoTracking()
                      .Where(x => x.UserId == currentUser.Id);

        if (year.HasValue || month.HasValue)
        {
            var period = CalculationPeriod.ForMonth(year!.Value, month!.Value);
            query = query.Where(x => x.From <= period.End && x.To >= period.Start);
        }

        var rows = await query.OrderByDescending(x => x.From).ToListAsync(cancellationToken);

        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }
}
