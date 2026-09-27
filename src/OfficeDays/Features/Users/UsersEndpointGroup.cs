using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Users;

public interface IUsersEndpoint : IEndpoint;

public sealed class UsersEndpointGroup : IEndpointGroup
{
    public void MapGroup(IEndpointGroupBuilder endpoints)
    {
        endpoints.MapEndpoints<IUsersEndpoint>("users");
    }
}
