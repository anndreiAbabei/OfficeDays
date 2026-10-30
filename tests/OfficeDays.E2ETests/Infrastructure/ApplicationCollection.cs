using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.E2ETests.Infrastructure;

[ExcludeFromCodeCoverage]
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    public const string Name = "OfficeDays E2E";
}
