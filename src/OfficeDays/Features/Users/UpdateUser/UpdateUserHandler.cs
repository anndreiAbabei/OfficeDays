using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserRequest>
{
    private readonly IOfficeRequirementReader _requirements;
    private readonly ILogger<UpdateUserHandler> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public UpdateUserHandler(ICurrentUser currentUser,
                             AppDbContext dbContext,
                             IUserService userService,
                             ILogger<UpdateUserHandler> logger,
                             IOfficeRequirementReader requirements)
    {
        _logger = logger;
        _requirements = requirements;
        _currentUser = currentUser;
        _dbContext = dbContext;
        _userService = userService;
    }
    
    public async ValueTask<IResult> Handle(UpdateUserRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var user = await _dbContext.Users.Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.Id == _currentUser.Id, cancellationToken);

        if (user is null)
        {
            _logger.LogUserNotFound(_currentUser.Id);
            return Results.NotFound();
        }

        user.Email = _userService.NormalizeEmail(request.Email);
        
        if (request.HasTeamDay)
            user.Preferences.TeamDay = request.TeamDay is null ? null : Enum.Parse<DayOfWeek>(request.TeamDay, true);

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        var requirement = await _requirements.CurrentRequirement(user, cancellationToken);
        var result = user.ToViewModel(requirement);

        return Results.Ok(result);
    }
}
