using OfficeDays.Features.Tokens.CreateToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.CreateToken;

public sealed class CreateTokenEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", _executor.Execute<CreateTokenRequest>)
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
