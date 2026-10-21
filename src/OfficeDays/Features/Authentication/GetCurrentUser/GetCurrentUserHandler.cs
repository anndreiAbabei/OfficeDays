using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly IOfficeRequirementReader _requirements;
    private readonly ILogger<GetCurrentUserHandler> _logger;
    
    public GetCurrentUserHandler(ICurrentUser currentUser,
                                 AppDbContext dbContext,
                                 IOfficeRequirementReader requirements,
                                 ILogger<GetCurrentUserHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _requirements = requirements;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(GetCurrentUserRequest input, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.Include(x => x.HolidayJurisdiction)
                                   .SingleOrDefaultAsync(x => x.Id == _currentUser.Id, cancellationToken);

        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.NotFound();
        }

        var requirement = await _requirements.CurrentRequirement(user, cancellationToken);
        var model = user.ToViewModel(requirement);
        
        return Results.Ok(model);
    }
}
