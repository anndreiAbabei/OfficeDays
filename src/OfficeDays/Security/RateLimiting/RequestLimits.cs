using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.Security.RateLimiting;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Loaded from appsettings.json")]
public sealed class RequestLimits
{
    public int MaxBodyBytes { get; init; } = 65_536;
    public int MaxPartitions { get; init; } = 10_000;
    public RequestLimit Source { get; init; } = new RequestLimit(600, 60);
    public RequestLimit Api { get; init; } = new RequestLimit(120, 60);
    public RequestLimit Token { get; init; } = new RequestLimit(60, 60);
    public RequestLimit Login { get; init; } = new RequestLimit(20, 300);
    public RequestLimit LoginAccount { get; init; } = new RequestLimit(10, 300);
    public RequestLimit Registration { get; init; } = new RequestLimit(5, 3600);
    public RequestLimit Recovery { get; init; } = new RequestLimit(10, 900);
    public RequestLimit RecoveryAccount { get; init; } = new RequestLimit(5, 900);
    public RequestLimit Verification { get; init; } = new RequestLimit(10, 900);
    public RequestLimit Reports { get; init; } = new RequestLimit(10, 60);
}

