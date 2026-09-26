using Microsoft.AspNetCore.Antiforgery;
using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.GetCsrfToken;

public sealed class GetCsrfTokenHandler(IAntiforgery antiforgery,
    IHttpContextAccessor contextAccessor) : IRequestHandler<GetCsrfTokenRequest>
{
    public ValueTask<IResult> Handle(GetCsrfTokenRequest input, CancellationToken cancellationToken)
    {
        var context = contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IResult>(Results.Ok(new GetCsrfTokenResponse(antiforgery.GetAndStoreTokens(context).RequestToken)));
    }
}
