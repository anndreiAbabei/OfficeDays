using OfficeDays.Middleware;

namespace OfficeDays.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseBearerTokenHttpsProtection(this IApplicationBuilder app) => app.UseMiddleware<BearerTokenHttpsMiddleware>();
}
