using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using OfficeDays.Configuration;
using OfficeDays.Features.Operations.GetConfiguration.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Operations.GetConfiguration;

public sealed class GetConfigurationHandler : IRequestHandler<GetConfigurationRequest>
{
    private readonly PrivacyOptions _privacy;
    private readonly TimeSpan _loginLifetime;

    public GetConfigurationHandler(IOptions<PrivacyOptions> privacy, IOptionsMonitor<CookieAuthenticationOptions> cookies)
    {
        _privacy = privacy.Value;
        _loginLifetime = cookies.Get(CookieAuthenticationDefaults.AuthenticationScheme).ExpireTimeSpan;
    }

    public ValueTask<IResult> Handle(GetConfigurationRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var notice = _privacy.Enabled 
                         ? new PublishedPrivacyNotice(_privacy.OperatorName, _privacy.ContactEmail, _privacy.OperatorNotice) 
                         : null;
        var response = new GetConfigurationResponse(notice, _loginLifetime.TotalSeconds);
        var result = Results.Ok(response);
        
        return ValueTask.FromResult(result);
    }
}
