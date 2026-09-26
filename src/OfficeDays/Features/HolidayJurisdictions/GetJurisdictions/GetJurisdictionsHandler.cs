using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsHandler(AppDbContext db) : IRequestHandler<GetJurisdictionsRequest>
{
    public async ValueTask<IResult> Handle(GetJurisdictionsRequest input, CancellationToken cancellationToken)
    {

        var rows = await db.HolidayJurisdictions.AsNoTracking()
                           .OrderBy(x => x.Name)
                           .ToListAsync(cancellationToken);

        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }
}
