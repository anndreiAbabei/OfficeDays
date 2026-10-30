using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Support;

namespace OfficeDays.E2ETests.Pages;

public sealed class LoginPage(IPage page) : BasePage(page), IScenarioPage<LoginPage>
{
    public static string Url => "/login";

    public ILocator UsernameInput => Page.GetByLabel("Username ", new PageGetByLabelOptions { Exact = true });
    public ILocator PasswordInput => Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true });
    public ILocator SigninButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true });
    
    public static LoginPage Create(IPage page) => new LoginPage(page);

    public async Task<DashboardPage> SignIn(TestUser user)
    {
        await UsernameInput.FillAsync(user.Username);
        await PasswordInput.FillAsync(user.Password);
        await SigninButton.ClickAsync();
        
        return await Page.Expects<DashboardPage>();
    }
}
