using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens.GetTokens;

public sealed class GetTokensEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", _executor.Execute<GetTokensRequest>);
    }
}
