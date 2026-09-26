using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.GetTokens;

public sealed class GetTokensHandler : IRequestHandler<GetTokensRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _dbContext;
    
    public GetTokensHandler(ICurrentUser currentUser,
                            AppDbContext dbContext)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
    }
    
    public async ValueTask<IResult> Handle(GetTokensRequest input, CancellationToken cancellationToken)
    {
        var rows = await _dbContext.ApiTokens
                                 .AsNoTracking()
                                 .Where(x => x.UserId == _currentUser.Id)
                                 .OrderByDescending(x => x.CreatedAt)
                                 .ToListAsync(cancellationToken);
        
        return Results.Ok(rows.ToViewModel());
    }
}
