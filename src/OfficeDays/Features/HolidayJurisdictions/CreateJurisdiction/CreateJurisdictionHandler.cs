using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions.CreateJurisdiction;

public sealed class CreateJurisdictionHandler(AppDbContext db,
    ILogger<CreateJurisdictionHandler> logger) : IRequestHandler<CreateJurisdictionRequest>
{
    public async ValueTask<IResult> Handle(CreateJurisdictionRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        HolidayJurisdictionCodes.TryNormalize(request.Code, out var code);
        if (await db.HolidayJurisdictions.AnyAsync(x => x.Code == code, cancellationToken))
            return Results.Conflict(new ProblemDetails
                { Status = StatusCodes.Status409Conflict, Title = "Holiday jurisdiction already exists" });

        var jurisdiction = new HolidayJurisdiction { Code = code, Name = request.Name!.Trim() };
        db.HolidayJurisdictions.Add(jurisdiction);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (ApiResults.IsUniqueViolation(exception))
        {
            logger.LogDuplicateJurisdiction(code);
            return Results.Conflict(new ProblemDetails
                { Status = StatusCodes.Status409Conflict, Title = "Holiday jurisdiction already exists" });
        }

        logger.LogJurisdictionCreated(jurisdiction.Code);
        return Results.Created($"/api/holiday-jurisdictions/{jurisdiction.Code}", jurisdiction.ToViewModel());
    }
}
