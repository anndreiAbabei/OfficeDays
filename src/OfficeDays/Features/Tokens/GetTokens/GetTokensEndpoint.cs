using OfficeDays.Features.Tokens.GetTokens.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Tokens.GetTokens;

public sealed class GetTokensEndpoint(IRequestExecutor executor) : ITokensEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", executor.Execute<GetTokensRequest>);
    }
}
