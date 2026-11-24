using OfficeDays.Features.Automation.GetIosAutomation.Contracts;
using OfficeDays.Infrastructure;

namespace OfficeDays.Features.Automation.GetIosAutomation;

public sealed class GetIosAutomationHandler(IConfiguration configuration) : IRequestHandler<GetIosAutomationRequest>
{
    private readonly IConfiguration _configuration = configuration;

    public ValueTask<IResult> Handle(GetIosAutomationRequest input, CancellationToken cancellationToken)
    {
        var configuredUrl = _configuration["Automation:Ios:ShortcutUrl"];
        var isShortcutLink = Uri.TryCreate(configuredUrl, UriKind.Absolute, out var url)
                             && url.Scheme == Uri.UriSchemeHttps
                             && url.Host is "www.icloud.com" or "icloud.com"
                             && url.UserInfo.Length == 0
                             && url.IsDefaultPort
                             && url.AbsolutePath.StartsWith("/shortcuts/", StringComparison.Ordinal)
                             && url.Segments.Length == 3;

        return ValueTask.FromResult<IResult>(Results.Ok(new GetIosAutomationResponse(isShortcutLink ? configuredUrl : null)));
    }
}
