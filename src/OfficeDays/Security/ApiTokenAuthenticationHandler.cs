using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeDays.Data;

namespace OfficeDays.Security;

public sealed class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiToken";
    private readonly AppDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ApiTokenAuthenticationHandler> _logger;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        AppDbContext db,
        TimeProvider timeProvider)
        : base(options, loggerFactory, encoder)
    {
        _db = db;
        _timeProvider = timeProvider;
        _logger = loggerFactory.CreateLogger<ApiTokenAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = header["Bearer ".Length..].Trim();
        if (rawToken.Length == 0)
        {
            return AuthenticateResult.Fail("A bearer token is required.");
        }

        var hash = ApiTokenService.Hash(rawToken);
        var token = await _db.ApiTokens.Include(x => x.User).ThenInclude(x => x.HolidayJurisdiction)
            .SingleOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAt == null);
        if (token is null)
        {
            _logger.LogWarning("Rejected an invalid or revoked API token from {RemoteIpAddress}",
                Context.Connection.RemoteIpAddress);
            return AuthenticateResult.Fail("The bearer token is invalid or revoked.");
        }

        token.LastUsedAt = _timeProvider.GetUtcNow();
        await _db.SaveChangesAsync();
        _logger.LogDebug("API token {TokenId} authenticated user {UserId}", token.Id, token.UserId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, token.UserId.ToString()),
            new Claim(ClaimTypes.Name, token.User.Username),
            new Claim("timezone", token.User.TimeZoneId),
            new Claim("country", token.User.HolidayJurisdiction.Code),
            new Claim("is_admin", token.User.IsAdmin ? "true" : "false")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }
}
