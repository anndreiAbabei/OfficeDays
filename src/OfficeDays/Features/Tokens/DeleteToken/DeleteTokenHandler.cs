using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.DeleteToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.DeleteToken;

public sealed class DeleteTokenHandler(ICurrentUser currentUser,
                                       AppDbContext db,
                                       ILogger<DeleteTokenHandler> logger)
    : IRequestHandler<DeleteTokenRequest>
{
    public async ValueTask<IResult> Handle(DeleteTokenRequest input, CancellationToken cancellationToken)
    {
        var token = await db.ApiTokens.SingleOrDefaultAsync(
            x => x.Id == input.Id && x.UserId == currentUser.Id, cancellationToken);

        if (token is null)
        {
            logger.LogTokenNotFound(input.Id, currentUser.Id);
            return Results.NotFound();
        }

        if (token.RevokedAt is null)
        {
            logger.LogTokenNotRevoked(token.Id, currentUser.Id);
            return Results.Conflict(new { detail = "Revoke the token before deleting it permanently." });
        }

        db.ApiTokens.Remove(token);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }
}
