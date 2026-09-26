using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Vacations.RemoveVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationHandler(ICurrentUser currentUser,
    AppDbContext db,
    ILogger<RemoveVacationHandler> logger) : IRequestHandler<RemoveVacationRequest>
{
    public async ValueTask<IResult> Handle(RemoveVacationRequest input, CancellationToken cancellationToken)
    {
        var id = input.Id;
        var userId = currentUser.Id;
        var row = await db.Vacations.SingleOrDefaultAsync(
            x => x.Id == id && x.UserId == userId, cancellationToken);
        if (row is null)
            return Results.NotFound();
        db.Vacations.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogVacationRemoved(userId, row.Id);
        return Results.NoContent();
    }
}
