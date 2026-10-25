using OfficeDays.E2ETests.Infrastructure;

namespace OfficeDays.E2ETests.Pages;

public sealed class ProfilePage(IPage page) : AuthenticatedPage(page), IScenarioPage<ProfilePage>
{
    public static string Url => "/profile";
    public static ProfilePage Create(IPage page) => new ProfilePage(page);

    public async Task SelectTeamDay(string expectedTeamDay, bool doSave = true)
    {
        await Page.GetByLabel("Team day").SelectOptionAsync([expectedTeamDay]);

        if (doSave)
            await Save();
    }
    
    public async Task Save()
    {
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save profile" }).ClickAsync();
    }
    
    public Task<string> GetTeamDay()
    {
        throw new NotImplementedException();
    }
}
