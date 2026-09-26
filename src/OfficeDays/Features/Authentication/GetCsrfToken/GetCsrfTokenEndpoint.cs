using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.GetCsrfToken;

public sealed class GetCsrfTokenEndpoint(IRequestExecutor executor) : IAuthenticationEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("csrf", executor.Execute<GetCsrfTokenRequest>).AllowAnonymous();
    }
}
