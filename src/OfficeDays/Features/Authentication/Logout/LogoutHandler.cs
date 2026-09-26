using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using OfficeDays.Features.Authentication.Logout.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.Logout;

public sealed class LogoutHandler(ICurrentUser currentUser,
    IHttpContextAccessor contextAccessor,
    ILogger<LogoutHandler> logger) : IRequestHandler<LogoutRequest>
{
    public async ValueTask<IResult> Handle(LogoutRequest input, CancellationToken cancellationToken)
    {
        var context = contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        var userId = currentUser.Id;

        cancellationToken.ThrowIfCancellationRequested();
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        logger.LogSignedOut(userId);

        return Results.NoContent();
    }
}
