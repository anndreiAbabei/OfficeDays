using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations;

public static class VacationEndpoints
{
    private const string LogCategory = "OfficeDays.Features.Vacations";

    public static void Map(RouteGroupBuilder api)
    {
        var vacations = api.MapGroup("/vacations").RequireAuthorization();
        vacations.MapGet("/", GetVacations);
        vacations.MapPost("/", CreateVacation).AddEndpointFilter<CookieAntiforgeryFilter>();
        vacations.MapDelete("/{id:guid}", RemoveVacation).AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> GetVacations(int? year,
                                                    int? month,
                                                    ClaimsPrincipal principal,
                                                    AppDbContext db,
                                                    CancellationToken cancellationToken)
    {
        var query = db.Vacations
                      .AsNoTracking()
                      .Where(x => x.UserId == principal.GetUserId());
        
        if (year.HasValue || month.HasValue)
        {
            if (!ApiResults.TryMonth(year, month, out var period, out var error)) 
                return error;
            query = query.Where(x => x.From <= period.End && x.To >= period.Start);
        }

        var rows = await query.OrderByDescending(x => x.From).ToListAsync(cancellationToken);
        
        return Results.Ok(rows.Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> CreateVacation(CreateVacationRequest request,
                                                      ClaimsPrincipal principal,
                                                      AppDbContext db,
                                                      TimeProvider clock,
                                                      ILoggerFactory loggerFactory,
                                                      CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var error = VacationValidation.Validate(request);
        if (error is not null) 
            return ApiResults.Validation(error, "to");
        
        var row = new Vacation
        {
            Id = Guid.NewGuid(),
            UserId = principal.GetUserId(),
            From = request.From,
            To = request.To,
            CreatedAt = clock.GetUtcNow()
        };
        db.Vacations.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} added vacation {VacationId} from {FromDate} to {ToDate}", row.UserId, row.Id, row.From, row.To);
        return Results.Created($"/api/vacations/{row.Id}", row.ToViewModel());
    }

    private static async Task<IResult> RemoveVacation(Guid id,
                                                      ClaimsPrincipal principal,
                                                      AppDbContext db,
                                                      ILoggerFactory loggerFactory,
                                                      CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var userId = principal.GetUserId();
        var row = await db.Vacations.SingleOrDefaultAsync(
            x => x.Id == id && x.UserId == userId, cancellationToken);
        if (row is null) 
            return Results.NotFound();
        db.Vacations.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} removed vacation {VacationId}", userId, row.Id);
        return Results.NoContent();
    }
}
