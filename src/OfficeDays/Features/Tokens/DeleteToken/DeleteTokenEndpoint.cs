using OfficeDays.Features.Tokens.DeleteToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.DeleteToken;

public sealed class DeleteTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("{id:guid}/permanent", executor.Execute<DeleteTokenRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
