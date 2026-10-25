using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;

namespace OfficeDays.E2ETests.Pages;

public abstract class BasePage(IPage page) : IScenarioPage
{
    protected IPage Page { get; } = page;

    public async Task<TPage> GoTo<TPage>()
        where TPage : IScenarioPage<TPage>
    {
        return await Page.GoTo<TPage>();
    }
}

public abstract class AuthenticatedPage(IPage page) : BasePage(page)
{
    public async Task<LoginPage> SignOut()
    {
        await Page.Locator("#log-out-btn").ClickAsync();
        
        return await Page.Expects<LoginPage>();
    }
}