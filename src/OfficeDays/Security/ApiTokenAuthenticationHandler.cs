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
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ApiTokenAuthenticationHandler> _logger;
    private readonly IApiTokenService _apiTokenService;

    public ApiTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                         ILoggerFactory loggerFactory,
                                         UrlEncoder encoder,
                                         AppDbContext dbContext,
                                         TimeProvider timeProvider,
                                         IApiTokenService apiTokenService)
        : base(options, loggerFactory, encoder)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = loggerFactory.CreateLogger<ApiTokenAuthenticationHandler>();
        _apiTokenService = apiTokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            Logging.ApiTokenAuthenticationHandlerLogging.LogNoBearerCredentials(_logger);
            return AuthenticateResult.NoResult();
        }

        var rawToken = header["Bearer ".Length..].Trim();
        if (rawToken.Length == 0)
        {
            Logging.ApiTokenAuthenticationHandlerLogging.LogMissingToken(_logger, Context.Connection.RemoteIpAddress);
            return AuthenticateResult.Fail("A bearer token is required.");
        }

        var hash = _apiTokenService.Hash(rawToken);
        var token = await _dbContext.ApiTokens.Include(x => x.User)
                                    .ThenInclude(x => x.HolidayJurisdiction)
                                    .SingleOrDefaultAsync(x => x.TokenHash == hash,
                                                          Context.RequestAborted);
        if (token is null)
        {
            Logging.ApiTokenAuthenticationHandlerLogging.LogInvalidToken(_logger, Context.Connection.RemoteIpAddress);
            return AuthenticateResult.Fail("The bearer token is invalid or revoked.");
        }

        if (token.RevokedAt is not null)
        {
            Logging.ApiTokenAuthenticationHandlerLogging.LogRevokedToken(_logger, token.Id, Context.Connection.RemoteIpAddress);
            return AuthenticateResult.Fail("The bearer token is invalid or revoked.");
        }

        token.LastUsedAt = _timeProvider.GetUtcNow();
        await _dbContext.SaveChangesAsync(Context.RequestAborted);
        Logging.ApiTokenAuthenticationHandlerLogging.LogTokenAuthenticated(_logger, token.Id, token.UserId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, token.UserId.ToString()),
            new Claim("api_token_id", token.Id.ToString()),
            new Claim(ClaimTypes.Name, token.User.Username),
            new Claim("timezone", token.User.TimeZoneId),
            new Claim("country", token.User.HolidayJurisdiction.Code),
            new Claim("is_admin", token.User.IsAdmin ? "true" : "false")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName));
    }
}
