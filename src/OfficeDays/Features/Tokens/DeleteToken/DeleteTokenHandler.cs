using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.DeleteToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.DeleteToken;

public sealed class DeleteTokenHandler : IRequestHandler<DeleteTokenRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _db;
    private readonly ILogger<DeleteTokenHandler> _logger;
    
    public DeleteTokenHandler(ICurrentUser currentUser,
                              AppDbContext db,
                              ILogger<DeleteTokenHandler> logger)
    {
        _currentUser = currentUser;
        _db = db;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(DeleteTokenRequest input, CancellationToken cancellationToken)
    {
        var token = await _db.ApiTokens.SingleOrDefaultAsync(
            x => x.Id == input.Id && x.UserId == _currentUser.Id, cancellationToken);

        if (token is null)
        {
            _logger.LogTokenNotFound(input.Id, _currentUser.Id);
            return Results.NotFound();
        }

        if (token.RevokedAt is null)
        {
            _logger.LogTokenNotRevoked(token.Id, _currentUser.Id);
            
            return Results.Conflict(new { detail = "Revoke the token before deleting it permanently." });
        }

        _db.ApiTokens.Remove(token);
        await _db.SaveChangesAsync(cancellationToken);
        
        return Results.NoContent();
    }
}
