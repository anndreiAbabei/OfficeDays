using OfficeDays.E2ETests.Infrastructure;

namespace OfficeDays.E2ETests.Pages;

public sealed class ProfilePage(IPage page) : AuthenticatedPage(page), IScenarioPage<ProfilePage>
{
    public static string Url => "/profile";
    public static ProfilePage Create(IPage page) => new ProfilePage(page);

    public ILocator TeamDaySelect => Page.GetByLabel("Team day");
    public ILocator EmailInput => Page.GetByLabel("Email address");
    public ILocator SaveButton => Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save profile" });
    public ILocator CurrentAndPastRequirements => Page.Locator(".current-attendances ~ div.requirement-row");
    public ILocator EditAttendancesForm => Page.Locator("form.requirement-form");

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
    
    public async Task SetEmail(string email, bool doSave = true)
    {
        await EmailInput.FillAsync(email);

        if (doSave)
            await Save();
    }
    
    public async Task Save() => await SaveButton.ClickAsync();
}
