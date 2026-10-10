using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Planning.RemovePlan.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Planning.RemovePlan;

public sealed class RemovePlanHandler(AppDbContext db, ICurrentUser currentUser, ILogger<RemovePlanHandler> logger) : IRequestHandler<RemovePlanRequest>
{
    public async ValueTask<IResult> Handle(RemovePlanRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(x => x.Id == currentUser.Id, cancellationToken)) 
            return Results.NotFound();
        
        await db.OfficePlans
                .Where(x => x.UserId == currentUser.Id && x.Date == request.Date)
                .ExecuteDeleteAsync(cancellationToken);
        
        logger.LogPlanChanged(currentUser.Id, request.Date);
        
        return Results.NoContent();
    }
}
