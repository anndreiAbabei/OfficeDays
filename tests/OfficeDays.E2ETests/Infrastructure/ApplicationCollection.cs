namespace OfficeDays.E2ETests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApplicationCollection : ICollectionFixture<ApplicationFixture>
{
    public const string Name = "OfficeDays E2E";
}
