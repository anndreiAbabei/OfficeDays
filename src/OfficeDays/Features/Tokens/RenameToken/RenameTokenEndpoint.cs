using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RenameToken;

public sealed class RenameTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("{id:guid}", _executor.Execute<RenameTokenRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
