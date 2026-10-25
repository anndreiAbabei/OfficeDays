using OfficeDays.E2ETests.Infrastructure;

namespace OfficeDays.E2ETests.Pages;

public sealed class ProfilePage(IPage page) : AuthenticatedPage(page), IScenarioPage<ProfilePage>
{
    public static string Url => "/profile";
    public static ProfilePage Create(IPage page) => new ProfilePage(page);

    public ILocator TeamDaySelect => Page.GetByLabel("Team day");
    public ILocator SaveButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save profile" });
    
    public async Task<string> GetTeamDay()
    {
        return await TeamDaySelect.InputValueAsync();
    }

    public async Task SelectTeamDay(string expectedTeamDay, bool doSave = true)
    {
        await TeamDaySelect.SelectOptionAsync([expectedTeamDay]);

        if (doSave)
            await Save();
    }
    
    public async Task Save() => await SaveButton.ClickAsync();
}
