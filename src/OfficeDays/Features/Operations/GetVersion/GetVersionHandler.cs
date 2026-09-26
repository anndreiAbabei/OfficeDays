using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Infrastructure;
using System.Reflection;

namespace OfficeDays.Features.Operations.GetVersion;

public sealed class GetVersionHandler(IHttpContextAccessor contextAccessor) : IRequestHandler<GetVersionRequest>
{
    private static readonly string Version = typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    public ValueTask<IResult> Handle(GetVersionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = contextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP request.");
        context.Response.Headers.CacheControl = "no-store";
        return ValueTask.FromResult<IResult>(Results.Ok(new GetVersionResponse(Version)));
    }
}
