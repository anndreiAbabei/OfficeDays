using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Common;
using OfficeDays.Features.Users;
using OfficeDays.Security;

namespace OfficeDays.Features.Authentication;

public static class AuthenticationEndpoints
{
    private const string LogCategory = "OfficeDays.Features.Authentication";

    public static void Map(RouteGroupBuilder api)
    {
        api.MapGet("/auth/csrf", GetCsrfToken).AllowAnonymous();
        api.MapPost("/auth/login", Login).AllowAnonymous();
        api.MapPost("/auth/logout", Logout)
           .RequireAuthorization()
           .AddEndpointFilter<CookieAntiforgeryFilter>();
        api.MapGet("/auth/me", GetCurrentUser).RequireAuthorization();
    }

    private static IResult GetCsrfToken(IAntiforgery antiforgery, HttpContext context) => Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });

    private static async Task<IResult> Login([FromBody] LoginRequest request,
                                             AppDbContext db,
                                             IPasswordHasher<User> hasher,
                                             HttpContext context,
                                             ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        
        if (!AuthenticationValidation.HasCredentials(request))
            return ApiResults.Validation("Username and password are required.");

        var normalized = ApiResults.NormalizeUsername(request.Username!);
        var user = await db.Users
                           .Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.NormalizedUsername == normalized);
        
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password!) == PasswordVerificationResult.Failed)
        {
            logger.LogWarning("Failed login attempt for username {Username} from {RemoteIpAddress}", 
                              request.Username, context.Connection.RemoteIpAddress);
            
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, 
                                   title: "Invalid credentials", 
                                   detail: "The username or password is incorrect.");
        }

        var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
        
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                                  new ClaimsPrincipal(identity), 
                                  new AuthenticationProperties { IsPersistent = true });
        
        logger.LogInformation("User {UserId} signed in", user.Id);
        
        return Results.Ok(user.ToViewModel());
    }

    private static async Task<IResult> Logout(ClaimsPrincipal principal,
                                              HttpContext context,
                                              ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(LogCategory);
        var userId = principal.GetUserId();
        
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        logger.LogInformation("User {UserId} signed out", userId);
        
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal principal, AppDbContext db)
    {
        var user = await db.Users
                           .Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.Id == principal.GetUserId());
        
        return user is null 
                   ? Results.NotFound() 
                   : Results.Ok(user.ToViewModel());
    }
}
