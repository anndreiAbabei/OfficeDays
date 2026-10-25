using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Support;

namespace OfficeDays.E2ETests.Pages;

public sealed class LoginPage(IPage page) : BasePage(page), IScenarioPage<LoginPage>
{
    public static string Url => "/login";
    public static LoginPage Create(IPage page) => new LoginPage(page);

    public async Task<DashboardPage> SignIn(TestUser user)
    {
        await Page.GetByLabel("Username", new PageGetByLabelOptions { Exact = true }).FillAsync(user.Username);
        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true }).FillAsync(user.Password);
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        
        return await Page.Expects<DashboardPage>();
    }
}
