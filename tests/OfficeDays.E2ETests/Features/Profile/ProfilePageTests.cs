using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Features.Profile;

[Collection(ApplicationCollection.Name)]
public sealed class ProfilePageTests(ApplicationFixture application, ITestOutputHelper output) : E2ETestBase(application, output) 
{
    [Fact]
    public async Task Should_DoLogout()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();

            // act
            await profile.SignOut();

            // assert
            await Expect(page).ToHaveURLAsync(LoginPage.Url);
        });
    }

    [Fact]
    public async Task Should_ShowVersionAndAuthor_WhenProfileIsOpened()
    {
        await RunScenarioAsync(async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var (expectedVersion, expectedAuthor, expectedRepositoryUrl) = await Application.Api.GetVersion(ct);

            var expectedText = $"Created by {expectedAuthor} · Office Days · v{expectedVersion}";

            // act
            var profile = await dashboard.GoToProfile();

            // assert
            var footer = profile.Footer;
            var authorLink = footer.GetByRole(AriaRole.Link,
                                              new LocatorGetByRoleOptions
                                              {
                                                  Name = expectedAuthor,
                                                  Exact = true
                                              });
            await Expect(footer).ToBeVisibleAsync();
            await Expect(footer).ToHaveTextAsync(expectedText);
            await Expect(authorLink).ToBeVisibleAsync();
            await Expect(authorLink).ToHaveAttributeAsync("href", expectedRepositoryUrl);
        });
    }
}
