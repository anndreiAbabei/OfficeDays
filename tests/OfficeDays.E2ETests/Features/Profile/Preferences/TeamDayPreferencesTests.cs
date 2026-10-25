using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;
using Xunit.Abstractions;

namespace OfficeDays.E2ETests.Features.Profile.Preferences;

[Collection(ApplicationCollection.Name)]
public sealed class TeamDayPreferencesTests(ApplicationFixture application, ITestOutputHelper output)  : E2ETestBase(application, output)
{
    [Fact]
    public async Task Should_UpdateUserPreferences()
    {
        await RunScenarioAsync(async (page, seeder) =>
        {
            // arrange
            const string expectedTeamDay = "tuesday";

            var user = await seeder.CreateUserAsync();
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
}
