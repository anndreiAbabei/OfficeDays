namespace OfficeDays.Features.Operations.GetVersion.Contracts;

public sealed record GetVersionResponse(string Version, string LatestVersion, string Author, string GitHubRepo)
{
    public bool NewVersionAvailable
    {
        get
        {
            var existingVersion = new Version(Version);
            var latestVersion = new Version(LatestVersion);
            
            return existingVersion > latestVersion;
        }
    }
}
