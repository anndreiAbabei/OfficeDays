using System.Diagnostics.CodeAnalysis;
using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;

namespace OfficeDays.E2ETests.Pages;

[ExcludeFromCodeCoverage]
public abstract class BasePage(IPage page) : IScenarioPage
{
    protected IPage Page { get; } = page;

    public async Task<TPage> GoTo<TPage>()
        where TPage : IScenarioPage<TPage>
    {
        return await Page.GoTo<TPage>();
    }
}

[ExcludeFromCodeCoverage]
public abstract class AuthenticatedPage(IPage page) : BasePage(page)
{
    public ILocator LogoutButton => Page.Locator("#log-out-btn");
    public ILocator Footer => Page.Locator("footer.app-footer");
    
    public async Task<LoginPage> SignOut()
    {
        await LogoutButton.ClickAsync();
        
        return await Page.Expects<LoginPage>();
    }
}