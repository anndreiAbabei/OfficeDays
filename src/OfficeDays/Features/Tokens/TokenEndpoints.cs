using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens;

public static class TokenEndpoints
{
    private const string LogCategory = "OfficeDays.Features.Tokens";

    public static void Map(RouteGroupBuilder api)
    {
        var tokens = api.MapGroup("/tokens").RequireAuthorization();
        tokens.MapGet("/", GetTokens);
        tokens.MapPost("/", CreateToken).AddEndpointFilter<CookieAntiforgeryFilter>();
        tokens.MapDelete("/{id:guid}", RevokeToken).AddEndpointFilter<CookieAntiforgeryFilter>();
    }

    private static async Task<IResult> GetTokens(ClaimsPrincipal principal, AppDbContext db)
    {
        var rows = await db.ApiTokens
            .AsNoTracking()
            .Where(x => x.UserId == principal.GetUserId())
            .ToListAsync();
        return Results.Ok(rows.OrderByDescending(x => x.CreatedAt).Select(x => x.ToViewModel()));
    }

    private static async Task<IResult> CreateToken(
        CreateTokenRequest request,
        ClaimsPrincipal principal,
        AppDbContext db,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var error = TokenValidation.ValidateName(request.Name);
        if (error is not null) return ApiResults.Validation(error, "name");

        var rawToken = ApiTokenService.Generate();
        var token = new ApiToken
        {
            Id = Guid.NewGuid(),
            UserId = principal.GetUserId(),
            Name = request.Name!.Trim(),
            TokenHash = ApiTokenService.Hash(rawToken),
            CreatedAt = timeProvider.GetUtcNow()
        };
        db.ApiTokens.Add(token);
        await db.SaveChangesAsync();
        logger.LogInformation("User {UserId} created API token {TokenId} named {TokenName}",
            token.UserId, token.Id, token.Name);
        return Results.Created($"/api/tokens/{token.Id}", token.ToCreatedViewModel(rawToken));
    }

    private static async Task<IResult> RevokeToken(
        Guid id,
        ClaimsPrincipal principal,
        AppDbContext db,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var userId = principal.GetUserId();
        var token = await db.ApiTokens.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (token is null) return Results.NotFound();
        if (token.RevokedAt is null)
        {
            token.RevokedAt = timeProvider.GetUtcNow();
            await db.SaveChangesAsync();
            logger.LogInformation("User {UserId} revoked API token {TokenId}", userId, token.Id);
        }
        return Results.NoContent();
    }
}
