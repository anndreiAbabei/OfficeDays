using System.Security.Claims;

namespace OfficeDays.Security;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)
                                                                               ?? throw new InvalidOperationException("Authenticated principal has no user identifier."));
}
