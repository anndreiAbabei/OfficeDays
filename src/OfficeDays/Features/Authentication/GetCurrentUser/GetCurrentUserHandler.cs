using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    
    public GetCurrentUserHandler(ICurrentUser currentUser, AppDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }
    
    public async ValueTask<IResult> Handle(GetCurrentUserRequest input, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.Include(x => x.HolidayJurisdiction)
                                   .SingleOrDefaultAsync(x => x.Id == _currentUser.Id, cancellationToken);

        if (user is null)
            return Results.NotFound();
        
        return Results.Ok(user.ToViewModel());
    }
}
