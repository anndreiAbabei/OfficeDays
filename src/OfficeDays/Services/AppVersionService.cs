using System.Reflection;
using OfficeDays.Extensions;

namespace OfficeDays.Services;

public interface IAppVersionService
{
    string Version { get; }
    string Author { get; }
    string GitHubRepo { get; }
    
    AppVersionInfo GetAll();
}

public sealed class AppVersionService : IAppVersionService
{
    private static readonly Assembly _assembly = typeof(Program).Assembly;
    
    public string Version => field ??= _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0-err";
    public string Author => field ??=  _assembly.GetMetadata("Authors", "Andrei");
    public string GitHubRepo => field ??= _assembly.GetMetadata("RepositoryUrl", "#");

    public AppVersionInfo GetAll() => new AppVersionInfo(Version, Author, GitHubRepo);
}

public readonly record struct AppVersionInfo(string Version, string Author, string GitHubRepo);
