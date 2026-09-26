using OfficeDays.Features.Tokens.RevokeToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RevokeToken;

public sealed class RevokeTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{id:guid}", executor.Execute<RevokeTokenRequest>).AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
