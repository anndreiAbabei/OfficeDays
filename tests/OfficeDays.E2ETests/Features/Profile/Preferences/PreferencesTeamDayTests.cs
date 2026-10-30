using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Features.Profile.Preferences;

[Collection(ApplicationCollection.Name)]
public sealed class PreferencesTeamDayTests(ApplicationFixture application, ITestOutputHelper output)  : E2ETestBase(application, output)
{
    [Fact]
    public async Task Should_TeamDaySetToNone_ByDefault()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedTeamDay = "";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);

            // act
            var profile = await dashboard.GoToProfile();

            // assert
            await Expect(profile.TeamDaySelect).ToHaveValueAsync(expectedTeamDay);
        });
    }
    
    [Fact]
    public async Task Should_UpdateUserPreferences()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedTeamDay = "tuesday";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);

            // act
            var profile = await dashboard.GoToProfile();
            await profile.SelectTeamDay(expectedTeamDay);
            await profile.SignOut();
            await login.SignIn(user);
            await dashboard.GoToProfile();

            // assert
            await Expect(profile.TeamDaySelect).ToHaveValueAsync(expectedTeamDay);
        });
    }
    
    
    [Fact]
    public async Task Should_SetUserPreferences_ToNone()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedTeamDay = "";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);

            // act
            var profile = await dashboard.GoToProfile();
            await profile.SelectTeamDay("tuesday");
            await profile.SignOut();
            await login.SignIn(user);
            await dashboard.GoToProfile();
            
            await profile.SelectTeamDay(expectedTeamDay);
            await profile.SignOut();
            await login.SignIn(user);
            await dashboard.GoToProfile();

            // assert
            await Expect(profile.TeamDaySelect).ToHaveValueAsync(expectedTeamDay);
        });
    }
}
