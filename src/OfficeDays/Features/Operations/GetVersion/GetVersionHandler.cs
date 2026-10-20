using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using System.Reflection;
using OfficeDays.Extensions;
using OfficeDays.Services;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionHandler : IRequestHandler<GetVersionRequest>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IAppVersionService _appVersion;
    private static readonly Assembly _assembly = typeof(Program).Assembly;
    
    private static readonly string _author = _assembly.GetMetadata("Authors", "Andrei");
    private static readonly string _githubRep = _assembly.GetMetadata("RepositoryUrl", "#");
    
    public GetVersionHandler(IHttpContextAccessor contextAccessor, IAppVersionService appVersion)
    {
        _contextAccessor = contextAccessor;
        _appVersion = appVersion;
    }

    public ValueTask<IResult> Handle(GetVersionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        
        context.Response.Headers.CacheControl = "no-store";

        var response = new GetVersionResponse(_appVersion.Version, _author, _githubRep);
        var result = Results.Ok(response);
        
        return ValueTask.FromResult(result);
    }
}
