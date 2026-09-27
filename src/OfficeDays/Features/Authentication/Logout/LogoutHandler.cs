using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using OfficeDays.Features.Authentication.Logout.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutRequest>
{
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<LogoutHandler> _logger;
    
    public LogoutHandler(ICurrentUser currentUser,
                         IHttpContextAccessor contextAccessor,
                         ILogger<LogoutHandler> logger)
    {
        _currentUser = currentUser;
        _contextAccessor = contextAccessor;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(LogoutRequest input, CancellationToken cancellationToken)
    {
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        var userId = _currentUser.Id;

        cancellationToken.ThrowIfCancellationRequested();
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogSignedOut(userId);

        return Results.NoContent();
    }
}
