using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;
using OfficeDays.Features.Users.Requirements;
using OfficeDays.Features.Users.GetRequirements.Contracts;

namespace OfficeDays.Features.Users.GetRequirements;

public sealed class GetRequirementsHandler : IRequestHandler<GetRequirementsRequest>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserDateService _dates;
    
    public GetRequirementsHandler(AppDbContext db, ICurrentUser currentUser, IUserDateService dates)
    {
        _db = db;
        _currentUser = currentUser;
        _dates = dates;
    }
    
    public async ValueTask<IResult> Handle(GetRequirementsRequest request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync([_currentUser.Id], cancellationToken);
        
        if (user is null)
            return Results.NotFound();
        
        var rows = await _db.UserRequiredOfficePercentages
                           .AsNoTracking()
                           .Where(x => x.UserId == user.Id)
                           .OrderByDescending(x => x.StartingFrom)
                           .ToListAsync(cancellationToken);
        
        return Results.Ok(new { today = _dates.Today(user), items = rows.Select(RequirementResponse.From) });
    }
}
