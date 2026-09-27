using OfficeDays.Features.Tokens.RevokeToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RevokeToken;

public sealed class RevokeTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{id:guid}", _executor.Execute<RevokeTokenRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
