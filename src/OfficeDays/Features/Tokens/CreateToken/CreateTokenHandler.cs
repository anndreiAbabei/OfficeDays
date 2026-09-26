using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenHandler : IRequestHandler<CreateTokenRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly AppDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CreateTokenHandler> _logger;
    
    public CreateTokenHandler(ICurrentUser currentUser,
                              AppDbContext db,
                              TimeProvider timeProvider,
                              ILogger<CreateTokenHandler> logger)
    {
        _currentUser = currentUser;
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(CreateTokenRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var rawToken = ApiTokenService.Generate();
        var token = new ApiToken
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.Id,
            Name = request.Name.Trim(),
            TokenHash = ApiTokenService.Hash(rawToken),
            CreatedAt = _timeProvider.GetUtcNow()
        };
        
        await _db.ApiTokens.AddAsync(token, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        
        _logger.LogTokenCreated(token.UserId, token.Id, token.Name);
        
        return Results.Created($"/api/tokens/{token.Id}", token.ToCreatedViewModel(rawToken));
    }
}
