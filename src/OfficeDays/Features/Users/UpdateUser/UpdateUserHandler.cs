using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly IUserService _userService;

    public UpdateUserHandler(ICurrentUser currentUser,
                             AppDbContext dbContext,
                             IUserService userService)
    {
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
            return Results.NotFound();

        user.Email = _userService.NormalizeEmail(request.Email);
        
        if (request.RequiredOfficePercentage.HasValue)
            user.RequiredOfficePercentage = request.RequiredOfficePercentage.Value;
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(user.ToViewModel());
    }
}
