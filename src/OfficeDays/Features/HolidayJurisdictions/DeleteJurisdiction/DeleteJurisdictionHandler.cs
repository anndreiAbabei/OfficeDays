using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.DeleteJurisdiction;

public sealed class DeleteJurisdictionHandler(AppDbContext db,
    ILogger<DeleteJurisdictionHandler> logger) : IRequestHandler<DeleteJurisdictionRequest>
{
    public async ValueTask<IResult> Handle(DeleteJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var code = input.Code;
        HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode);

        var jurisdiction = await db.HolidayJurisdictions.SingleOrDefaultAsync(
            x => x.Code == normalizedCode, cancellationToken);
        if (jurisdiction is null) return Results.NotFound();

        var inUse = await db.Users.AnyAsync(
                        x => x.HolidayJurisdictionId == jurisdiction.Id, cancellationToken) ||
                    await db.BankHolidays.AnyAsync(
                        x => x.HolidayJurisdictionId == jurisdiction.Id, cancellationToken);
        if (inUse)
            return Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Holiday jurisdiction is in use",
                Detail = "Remove its bank holidays and user references before deleting it."
            });

        db.HolidayJurisdictions.Remove(jurisdiction);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogJurisdictionDeleted(jurisdiction.Code);
        return Results.NoContent();
    }
}
