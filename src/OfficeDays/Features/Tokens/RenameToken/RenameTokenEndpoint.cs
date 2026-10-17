using OfficeDays.Features.Tokens.RenameToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.RenameToken;

public sealed class RenameTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPatch("{id:guid}", executor.Execute<RenameTokenRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
