using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.GetCsrfToken;

public sealed class GetCsrfTokenEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    private readonly IRequestExecutor _executor = executor;
    
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("csrf", _executor.Execute<GetCsrfTokenRequest>)
                 .AllowAnonymous();
    }
}
