namespace OfficeDays.Features.Operations.GetVersion.Contracts;

public sealed record GetVersionResponse(string Version, string LatestVersion, string Author, string GitHubRepo);
