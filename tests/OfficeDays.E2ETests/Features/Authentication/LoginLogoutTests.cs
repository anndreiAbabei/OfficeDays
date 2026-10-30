using System.Text.RegularExpressions;
using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Features.Authentication;

[Collection(ApplicationCollection.Name)]
public sealed partial class LoginLogoutTests : E2ETestBase
{
    public LoginLogoutTests(ApplicationFixture application, ITestOutputHelper output)
        : base(application, output)
    {
    }

    [Fact]
    public async Task Should_AllowSigningInAgain_WhenUserSignsOut()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // Arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            await login.SignIn(user);

            // Act
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign out", Exact = true }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true })).ToBeVisibleAsync();
            await page.GotoAsync("/profile");
            await Expect(page).ToHaveURLAsync(LoginRegex());
            await login.SignIn(user);

            // Assert
            await Expect(page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign out", Exact = true })).ToBeVisibleAsync();
        });
    }

    [GeneratedRegex("/login$")]
    private static partial Regex LoginRegex();
}
