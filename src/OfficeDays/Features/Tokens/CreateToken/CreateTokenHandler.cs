using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenHandler(ICurrentUser currentUser,
    AppDbContext db,
    TimeProvider timeProvider,
    ILogger<CreateTokenHandler> logger) : IRequestHandler<CreateTokenRequest>
{
    public async ValueTask<IResult> Handle(CreateTokenRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;

        var rawToken = ApiTokenService.Generate();
        var token = new ApiToken
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            Name = request.Name!.Trim(),
            TokenHash = ApiTokenService.Hash(rawToken),
            CreatedAt = timeProvider.GetUtcNow()
        };
        db.ApiTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogTokenCreated(token.UserId, token.Id, token.Name);
        return Results.Created($"/api/tokens/{token.Id}", token.ToCreatedViewModel(rawToken));
    }
}
