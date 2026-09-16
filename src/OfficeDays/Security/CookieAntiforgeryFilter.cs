using Microsoft.AspNetCore.Antiforgery;

namespace OfficeDays.Security;

public sealed class CookieAntiforgeryFilter : IEndpointFilter
{
    private readonly IAntiforgery _antiforgery;

    public CookieAntiforgeryFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }

        return await next(context);
    }
}
