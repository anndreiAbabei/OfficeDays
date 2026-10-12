namespace OfficeDays.Features.Operations.GetVersion.Contracts;

public sealed record GetVersionResponse(string Version, string Author, string GitHubRepo);
