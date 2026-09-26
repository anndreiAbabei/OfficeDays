using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.RevokeToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RevokeToken;

public sealed class RevokeTokenHandler : IRequestHandler<RevokeTokenRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RevokeTokenHandler> _logger;
    
    public RevokeTokenHandler(ICurrentUser currentUser,
                              AppDbContext dbContext,
                              TimeProvider timeProvider,
                              ILogger<RevokeTokenHandler> logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(RevokeTokenRequest input, CancellationToken cancellationToken)
    {
        var id = input.Id;
        var userId = _currentUser.Id;
        var token = await _dbContext.ApiTokens.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        
        if (token is null) 
            return Results.NotFound();
        
        if (token.RevokedAt is null)
        {
            token.RevokedAt = _timeProvider.GetUtcNow();
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogTokenRevoked(userId, token.Id);
        }
        
        return Results.NoContent();
    }
}
