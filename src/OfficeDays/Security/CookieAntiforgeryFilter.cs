using Microsoft.AspNetCore.Antiforgery;

namespace OfficeDays.Security;

public sealed class CookieAntiforgeryFilter : IEndpointFilter
{
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<CookieAntiforgeryFilter> _logger;

    public CookieAntiforgeryFilter(IAntiforgery antiforgery, ILogger<CookieAntiforgeryFilter> logger)
    {
        _antiforgery = antiforgery;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (AntiforgeryValidationException exception)
            {
                _logger.LogValidationFailed(exception, context.HttpContext.Request.Path.Value ?? "/");
                return Results.Problem(statusCode: StatusCodes.Status400BadRequest, 
                                       title: "Invalid antiforgery token",
                                       detail: "Refresh the page and try again.");
            }
        }

        return await next(context);
    }
}
