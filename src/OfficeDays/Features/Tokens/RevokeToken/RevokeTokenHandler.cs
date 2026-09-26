using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.RevokeToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RevokeToken;

public sealed class RevokeTokenHandler(ICurrentUser currentUser,
    AppDbContext db,
    TimeProvider timeProvider,
    ILogger<RevokeTokenHandler> logger) : IRequestHandler<RevokeTokenRequest>
{
    public async ValueTask<IResult> Handle(RevokeTokenRequest input, CancellationToken cancellationToken)
    {
        var id = input.Id;
        var userId = currentUser.Id;
        var token = await db.ApiTokens.SingleOrDefaultAsync(
            x => x.Id == id && x.UserId == userId, cancellationToken);
        if (token is null) return Results.NotFound();
        if (token.RevokedAt is null)
        {
            token.RevokedAt = timeProvider.GetUtcNow();
            await db.SaveChangesAsync(cancellationToken);
            logger.LogTokenRevoked(userId, token.Id);
        }
        return Results.NoContent();
    }
}
