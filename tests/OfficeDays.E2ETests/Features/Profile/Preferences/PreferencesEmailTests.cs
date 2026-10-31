using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Features.Profile.Preferences;

[Collection(ApplicationCollection.Name)]
public sealed class PreferencesEmailTests(ApplicationFixture application, ITestOutputHelper output) : E2ETestBase(application, output)
{
    [Fact]
    public async Task Should_HaveEmailEmpty_ByDefault()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedEmail = "";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);

            // act
            var profile = await dashboard.GoToProfile();

            // assert
            await Expect(profile.EmailInput).ToHaveValueAsync(expectedEmail);
        });
    }

    [Fact]
    public async Task Should_SetTheEmail_WhenEmailIsEmpty()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedEmail = "andrew@local.test";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();

            // act
            await profile.SetEmail(expectedEmail);

            // assert
            await profile.SignOut();
            await login.SignIn(user);
            await dashboard.GoToProfile();
            await Expect(profile.EmailInput).ToHaveValueAsync(expectedEmail);
        });
    }

    [Fact]
    public async Task Should_UpdateTheEmail_WhenEmailAlreadyExists()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            const string expectedEmail = "andrew@local.test";

            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();
            await profile.SetEmail("none@local-mail.test");

            // act
            await profile.SetEmail(expectedEmail);

            // assert
            await profile.SignOut();
            await login.SignIn(user);
            await dashboard.GoToProfile();
            await Expect(profile.EmailInput).ToHaveValueAsync(expectedEmail);
        });
    }
}
