using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.E2ETests.Infrastructure.Models;

[ExcludeFromCodeCoverage]
public sealed record GetVersionResponse(string Version, string Author, string GitHubRepo);