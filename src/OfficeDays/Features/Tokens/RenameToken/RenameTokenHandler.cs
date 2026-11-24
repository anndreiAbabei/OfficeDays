using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RenameToken;

public sealed class RenameTokenHandler : IRequestHandler<RenameTokenRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _db;
    private readonly ILogger<RenameTokenHandler> _logger;

    public RenameTokenHandler(ICurrentUser currentUser, AppDbContext db, ILogger<RenameTokenHandler> logger)
    {
        _currentUser = currentUser;
        _db = db;
        _logger = logger;
    }

    public async ValueTask<IResult> Handle(RenameTokenRequest input, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id;
        var ownedToken = _db.ApiTokens.Where(token => token.Id == input.Id && token.UserId == userId);
        var name = input.Body.Name.Trim();
        var updated = await ownedToken.Where(token => token.RevokedAt == null)
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.Name, name), cancellationToken);
        if (updated > 0)
            return Results.NoContent();

        if (await ownedToken.AnyAsync(cancellationToken))
        {
            _logger.LogRenameRejectedForRevokedToken(input.Id, userId);
            
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, detail: "Revoked tokens cannot be renamed.");
        }

        _logger.LogTokenNotFound(input.Id, userId);
        
        return Results.NotFound();
    }
}
