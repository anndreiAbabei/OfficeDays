using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;

namespace OfficeDays.E2ETests.Pages;

public sealed class DashboardPage(IPage page) : AuthenticatedPage(page), IScenarioPage<DashboardPage>
{
    public static string Url => "/";
    public static DashboardPage Create(IPage page) => new DashboardPage(page);

    public async Task<ProfilePage> GoToProfile()
    {
        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Profile" }).ClickAsync();

        return await Page.Expects<ProfilePage>();
    }
}
