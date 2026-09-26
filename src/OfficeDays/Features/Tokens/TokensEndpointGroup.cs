using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Tokens;

public interface ITokensEndpoint : IEndpoint;

public sealed class TokensEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<ITokensEndpoint>("tokens").RequireAuthorization();
    }
}
