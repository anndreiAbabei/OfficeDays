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
        var token = await _db.ApiTokens.SingleOrDefaultAsync(x => x.Id == input.Id && 
                                                                  x.UserId == _currentUser.Id, 
                                                             cancellationToken);

        if (token is null)
        {
            _logger.LogTokenNotFound(input.Id, _currentUser.Id);
            
            return Results.NotFound();
        }

        token.Name = input.Body.Name.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
