namespace OfficeDays.E2ETests.Infrastructure;

public interface IScenarioPage
{
    Task<TPage> GoTo<TPage>()
        where TPage : IScenarioPage<TPage>;
}

public interface IScenarioPage<out TPage> : IScenarioPage
    where TPage : IScenarioPage<TPage>
{
    static abstract string Url { get; }
    static abstract TPage Create(IPage page);
}
