using System.Reflection;

namespace OfficeDays.Services;

public interface IAppVersionService
{
    string Version { get; }
}

public sealed class AppVersionService : IAppVersionService
{
    public string Version { get; } = typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0-err";
}
