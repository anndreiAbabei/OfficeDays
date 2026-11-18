namespace OfficeDays.Features.Operations.GetVersion.Contracts;

public sealed record GetVersionResponse(string Version, string LatestVersion, string Author, string GitHubRepo)
{
    public bool NewVersionAvailable
    {
        get
        {
            var cleanVer = Version.Split('+', 2).FirstOrDefault();
            var cleanLatestVer = LatestVersion.Split('+', 2).FirstOrDefault();
            
            return System.Version.TryParse(cleanVer,        out var installed) &&
                   System.Version.TryParse(cleanLatestVer,  out var latest) &&
                   latest > installed;
        }
    }
}
