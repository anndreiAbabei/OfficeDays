using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;
using OfficeDays.Features.Users.GetRequirements.Contracts;

namespace OfficeDays.Features.Users.GetRequirements;

public sealed class GetRequirementsHandler : IRequestHandler<GetRequirementsRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserDateService _dates;
    private readonly ILogger<GetRequirementsHandler> _logger;
    
    public GetRequirementsHandler(AppDbContext db, 
                                  ICurrentUser currentUser, 
                                  IUserDateService dates, 
                                  ILogger<GetRequirementsHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _dates = dates;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(GetRequirementsRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([_currentUser.Id], cancellationToken);
        
        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            
            return Results.NotFound();
        }
        
        var rows = await _db.UserAttendanceRequirements
                           .AsNoTracking()
                           .Where(x => x.UserId == user.Id)
                           .OrderByDescending(x => x.StartingFrom)
                           .ToListAsync(cancellationToken);
        var result = user.ToViewModel(rows, _dates);

        return Results.Ok(result);
    }
}
