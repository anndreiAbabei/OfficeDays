using Microsoft.AspNetCore.Antiforgery;
using OfficeDays.Features.Authentication.GetCsrfToken.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Authentication.GetCsrfToken;

public sealed class GetCsrfTokenHandler : IRequestHandler<GetCsrfTokenRequest>
{
    private readonly IAntiforgery _antiforgery;
    private readonly IHttpContextAccessor _contextAccessor;
    
    public GetCsrfTokenHandler(IAntiforgery antiforgery, IHttpContextAccessor contextAccessor)
    {
        _antiforgery = antiforgery;
        _contextAccessor = contextAccessor;
    }
    
    public ValueTask<IResult> Handle(GetCsrfTokenRequest input, CancellationToken cancellationToken)
    {
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        cancellationToken.ThrowIfCancellationRequested();

        var result = Results.Ok(new GetCsrfTokenResponse(_antiforgery.GetAndStoreTokens(context).RequestToken));
        
        return ValueTask.FromResult(result);
    }
}
