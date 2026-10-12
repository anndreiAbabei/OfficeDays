using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using System.Reflection;
using OfficeDays.Extensions;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionHandler : IRequestHandler<GetVersionRequest>
{
    private readonly IHttpContextAccessor _contextAccessor;
    private static readonly Assembly _assembly = typeof(Program).Assembly;
    
    private const string Author = "https://github.com/anndreiAbabei/OfficeDays";
    private static readonly string _author = _assembly.GetMetadata("Authors", "Andrei");
    private static readonly string _githubRep = _assembly.GetMetadata("RepositoryUrl", "#");
    private static readonly string _version = _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                                  .InformationalVersion ?? "0.0.0-err";
    
    public GetVersionHandler(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public ValueTask<IResult> Handle(GetVersionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var context = _contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        
        context.Response.Headers.CacheControl = "no-store";

        var response = new GetVersionResponse(_version, _author, _githubRep);
        var result = Results.Ok(response);
        
        return ValueTask.FromResult(result);
    }
}
