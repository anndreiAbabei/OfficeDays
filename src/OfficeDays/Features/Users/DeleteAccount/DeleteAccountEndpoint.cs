using OfficeDays.Features.Users.DeleteAccount.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Security.RateLimiting;

namespace OfficeDays.Features.Users.DeleteAccount;

public sealed class DeleteAccountEndpoint(IRequestExecutor executor) : IUsersEndpoint
{
    private readonly IRequestExecutor _executor = executor;

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("me", _executor.Execute<DeleteAccountRequest>)
                 .RequireAuthorization("BrowserOnly")
                 .WithMetadata(new SensitiveRequestLimit("login"))
                 .AddEndpointFilter<CookieAntiforgeryFilter>();
    }
}
