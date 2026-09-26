using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.GetTokens;

public sealed class GetTokensHandler(ICurrentUser currentUser,
    AppDbContext db) : IRequestHandler<GetTokensRequest>
{
    public async ValueTask<IResult> Handle(GetTokensRequest input, CancellationToken cancellationToken)
    {

        var rows = await db.ApiTokens
            .AsNoTracking()
            .Where(x => x.UserId == currentUser.Id)
            .ToListAsync(cancellationToken);
        return Results.Ok(rows.OrderByDescending(x => x.CreatedAt).Select(x => x.ToViewModel()));
    }
}
