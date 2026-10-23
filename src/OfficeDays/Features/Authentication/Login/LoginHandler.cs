using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Authentication.Login.Contracts;
using OfficeDays.Infrastructure;
using System.Security.Claims;
using OfficeDays.Services;

namespace OfficeDays.Features.Authentication.Login;

public sealed class LoginHandler : IRequestHandler<LoginRequest>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;
    private readonly IOfficeRequirementReader _requirements;
    private readonly ILogger<LoginHandler> _logger;
    
    public LoginHandler(AppDbContext db,
                        IPasswordHasher<User> hasher,
                        IHttpContextAccessor contextAccessor,
                        IUserService userService,
                        IOfficeRequirementReader requirements,
                        ILogger<LoginHandler> logger)
    {
        _db = db;
        _hasher = hasher;
        _contextAccessor = contextAccessor;
        _userService = userService;
        _requirements = requirements;
        _logger = logger;
    }
    
    public async ValueTask<IResult> Handle(LoginRequest input, CancellationToken cancellationToken)
    {
        var request = input.Body;
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");

        var normalized = _userService.NormalizeUsername(request.Username);
        var user = await _db.Users
                           .Include(x => x.HolidayJurisdiction)
                           .SingleOrDefaultAsync(x => x.NormalizedUsername == normalized, cancellationToken);

        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            _logger.LogFailedLogin(request.Username, context.Connection.RemoteIpAddress);

            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized,
                                   title: "Invalid credentials",
                                   detail: "The username or password is incorrect.");
        }

        var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);

        cancellationToken.ThrowIfCancellationRequested();
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                  new ClaimsPrincipal(identity),
                                  new AuthenticationProperties { IsPersistent = true });

        _logger.LogSignedIn(user.Id);
        var requirement = await _requirements.CurrentRequirement(user, cancellationToken);
        var result = user.ToViewModel(requirement);

        return Results.Ok(result);
    }
}
