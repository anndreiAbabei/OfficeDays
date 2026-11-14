using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Planning.RemovePlan.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Planning.RemovePlan;

public sealed class RemovePlanHandler : IRequestHandler<RemovePlanRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<RemovePlanHandler> _logger;
    
    public RemovePlanHandler(AppDbContext db, ICurrentUser currentUser, ILogger<RemovePlanHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(RemovePlanRequest request, CancellationToken cancellationToken)
    {
        if (!await _db.Users.AnyAsync(x => x.Id == _currentUser.Id, cancellationToken))
        {
            _logger.LogUserNotFound(_currentUser.Id);
            
            return Results.NotFound();
        }

        await _db.OfficePlans
                .Where(x => x.UserId == _currentUser.Id && x.Date == request.Date)
                .ExecuteDeleteAsync(cancellationToken);

        _logger.LogPlanChanged(_currentUser.Id, request.Date);

        return Results.NoContent();
    }
}
