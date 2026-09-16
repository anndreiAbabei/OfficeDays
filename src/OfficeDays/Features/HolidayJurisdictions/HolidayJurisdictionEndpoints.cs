using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.HolidayJurisdictions;

public static class HolidayJurisdictionEndpoints
{
    private const string LogCategory = "OfficeDays.Features.HolidayJurisdictions";

    public static void Map(RouteGroupBuilder api)
    {
        var jurisdictions = api.MapGroup("/holiday-jurisdictions");
        jurisdictions.MapGet("/", GetJurisdictions).AllowAnonymous();
        jurisdictions.MapPost("/", CreateJurisdiction)
                     .RequireAuthorization("AdminOnly")
                     .AddEndpointFilter<CookieAntiforgeryFilter>();
        jurisdictions.MapPut("/{code}", UpdateJurisdiction)
                     .RequireAuthorization("AdminOnly")
                     .AddEndpointFilter<CookieAntiforgeryFilter>();
        jurisdictions.MapDelete("/{code}", DeleteJurisdiction)
                     .RequireAuthorization("AdminOnly")
                     .AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> GetJurisdictions(AppDbContext db, CancellationToken cancellationToken)
    {
        var rows = await db.HolidayJurisdictions.AsNoTracking()
                           .OrderBy(x => x.Name)
                           .ToListAsync(cancellationToken);
        
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> CreateJurisdiction([FromBody] CreateHolidayJurisdictionRequest request,
                                                          AppDbContext db,
                                                          ILoggerFactory loggerFactory,
                                                          CancellationToken cancellationToken)
    {
        var validation = HolidayJurisdictionValidation.Validate(request.Code, request.Name);
        if (validation.HasValue)
            return ApiResults.Validation(validation.Value.Message, validation.Value.Key);

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
            loggerFactory.CreateLogger(LogCategory)
                         .LogWarning("A duplicate holiday jurisdiction was attempted for {JurisdictionCode}", code);
            return Results.Conflict(new ProblemDetails
                { Status = StatusCodes.Status409Conflict, Title = "Holiday jurisdiction already exists" });
        }

        loggerFactory.CreateLogger(LogCategory)
                     .LogInformation("Created holiday jurisdiction {JurisdictionCode}", jurisdiction.Code);
        return Results.Created($"/api/holiday-jurisdictions/{jurisdiction.Code}", jurisdiction.ToViewModel());
    }

    private static async Task<IResult> UpdateJurisdiction(string code,
                                                          [FromBody] UpdateHolidayJurisdictionRequest request,
                                                          AppDbContext db,
                                                          ILoggerFactory loggerFactory,
                                                          CancellationToken cancellationToken)
    {
        if (!HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode))
            return ApiResults.Validation("A valid country or subdivision code is required.", "code");
        var validation = HolidayJurisdictionValidation.ValidateName(request.Name);
        if (validation.HasValue)
            return ApiResults.Validation(validation.Value.Message, validation.Value.Key);

        var jurisdiction = await db.HolidayJurisdictions.SingleOrDefaultAsync(
            x => x.Code == normalizedCode, cancellationToken);
        if (jurisdiction is null) return Results.NotFound();

        jurisdiction.Name = request.Name!.Trim();
        await db.SaveChangesAsync(cancellationToken);
        loggerFactory.CreateLogger(LogCategory)
                     .LogInformation("Updated holiday jurisdiction {JurisdictionCode}", jurisdiction.Code);
        return Results.Ok(jurisdiction.ToViewModel());
    }

    private static async Task<IResult> DeleteJurisdiction(string code,
                                                          AppDbContext db,
                                                          ILoggerFactory loggerFactory,
                                                          CancellationToken cancellationToken)
    {
        if (!HolidayJurisdictionCodes.TryNormalize(code, out var normalizedCode))
            return ApiResults.Validation("A valid country or subdivision code is required.", "code");

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
        loggerFactory.CreateLogger(LogCategory)
                     .LogInformation("Deleted holiday jurisdiction {JurisdictionCode}", jurisdiction.Code);
        return Results.NoContent();
    }
}
