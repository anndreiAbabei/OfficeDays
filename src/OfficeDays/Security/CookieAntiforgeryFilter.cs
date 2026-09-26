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
            try
            {
                await _antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, 
                                       title: "Invalid antiforgery token",
                                       detail: "Refresh the page and try again.");
            }
        }

        return await next(context);
    }
}
