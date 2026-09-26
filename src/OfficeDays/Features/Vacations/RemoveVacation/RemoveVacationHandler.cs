using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Vacations.RemoveVacation.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Vacations.RemoveVacation;

public sealed class RemoveVacationHandler : IRequestHandler<RemoveVacationRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RemoveVacationHandler> _logger;
    
    public RemoveVacationHandler(ICurrentUser currentUser,
                                 AppDbContext dbContext,
                                 ILogger<RemoveVacationHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(RemoveVacationRequest input, CancellationToken cancellationToken)
    {
        var id = input.Id;
        var userId = _currentUser.Id;
        var row = await _dbContext.Vacations.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        
        if (row is null)
            return Results.NotFound();
        
        _dbContext.Vacations.Remove(row);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogVacationRemoved(userId, row.Id);
        
        return Results.NoContent();
    }
}
