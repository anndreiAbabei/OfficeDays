using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Features.Common;
using OfficeDays.Infrastructure;
using System.Security.Claims;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginHandler(AppDbContext db,
    IPasswordHasher<User> hasher,
    IHttpContextAccessor contextAccessor,
    ILogger<LoginHandler> logger) : IRequestHandler<LoginRequest>
{
    public async ValueTask<IResult> Handle(LoginRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;
        var context = contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");

        var normalized = ApiResults.NormalizeUsername(request.Username!);
        var user = await db.Users
                           .Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.NormalizedUsername == normalized, cancellationToken);

        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password!) == PasswordVerificationResult.Failed)
        {
            logger.LogFailedLogin(request.Username, context.Connection.RemoteIpAddress);

            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized,
                                   title: "Invalid credentials",
                                   detail: "The username or password is incorrect.");
        }

        var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);

        cancellationToken.ThrowIfCancellationRequested();
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                  new ClaimsPrincipal(identity),
                                  new AuthenticationProperties { IsPersistent = true });

        logger.LogSignedIn(user.Id);

        return Results.Ok(user.ToViewModel());
    }
}
