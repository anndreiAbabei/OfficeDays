using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.UpdateJurisdiction;

public sealed class UpdateJurisdictionHandler(AppDbContext db,
    ILogger<UpdateJurisdictionHandler> logger) : IRequestHandler<UpdateJurisdictionRequest>
{
    public async ValueTask<IResult> Handle(UpdateJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var code = input.Code;
        var request = input.Body;
        HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode);

        var jurisdiction = await db.HolidayJurisdictions.SingleOrDefaultAsync(
            x => x.Code == normalizedCode, cancellationToken);
        if (jurisdiction is null) return Results.NotFound();

        jurisdiction.Name = request.Name!.Trim();
        await db.SaveChangesAsync(cancellationToken);
        logger.LogJurisdictionUpdated(jurisdiction.Code);
        return Results.Ok(jurisdiction.ToViewModel());
    }
}
