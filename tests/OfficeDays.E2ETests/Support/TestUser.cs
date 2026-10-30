using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.E2ETests.Support;

[ExcludeFromCodeCoverage]
public sealed record TestUser(string Username, string Password);
