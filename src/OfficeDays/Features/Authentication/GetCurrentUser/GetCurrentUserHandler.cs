using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Authentication.GetCurrentUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.GetCurrentUser;

public sealed class GetCurrentUserHandler(ICurrentUser currentUser,
    AppDbContext db) : IRequestHandler<GetCurrentUserRequest>
{
    public async ValueTask<IResult> Handle(GetCurrentUserRequest input, CancellationToken cancellationToken)
    {

        var user = await db.Users
                           .Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        return user is null
                   ? Results.NotFound()
                   : Results.Ok(user.ToViewModel());
    }
}
