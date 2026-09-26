using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", executor.Execute<CreateTokenRequest>).AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
