using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RenameToken;

public sealed class RenameTokenHandler(ICurrentUser currentUser, AppDbContext db)
    : IRequestHandler<RenameTokenRequest>
{
    public async ValueTask<IResult> Handle(RenameTokenRequest input, CancellationToken cancellationToken)
    {
        var token = await db.ApiTokens.SingleOrDefaultAsync(
            x => x.Id == input.Id && x.UserId == currentUser.Id, cancellationToken);

        if (token is null)
            return Results.NotFound();

        token.Name = input.Body.Name.Trim();
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
