using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Users.UpdateUser.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Users.UpdateUser;

public sealed class UpdateUserHandler(ICurrentUser currentUser,
    AppDbContext db) : IRequestHandler<UpdateUserRequest>
{
    public async ValueTask<IResult> Handle(UpdateUserRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var user = await db.Users.Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

        if (user is null)
            return Results.NotFound();

        user.Email = UserFields.NormalizeEmail(request.Email);
        if (request.RequiredOfficePercentage.HasValue)
            user.RequiredOfficePercentage = request.RequiredOfficePercentage.Value;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(user.ToViewModel());
    }
}
