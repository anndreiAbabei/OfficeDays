using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication;

public interface IAuthenticationEndpoint : IEndpoint;

public sealed class AuthenticationEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IAuthenticationEndpoint>("auth");
    }
}
