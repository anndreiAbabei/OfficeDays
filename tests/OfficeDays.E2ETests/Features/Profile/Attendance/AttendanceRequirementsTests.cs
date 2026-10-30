using OfficeDays.E2ETests.Infrastructure;
using OfficeDays.E2ETests.Infrastructure.Extensions;
using OfficeDays.E2ETests.Pages;

namespace OfficeDays.E2ETests.Features.Profile.Attendance;

[Collection(ApplicationCollection.Name)]
public sealed class AttendanceRequirementsTests(ApplicationFixture application, ITestOutputHelper output) : E2ETestBase(application, output)
{
    [Fact]
    public async Task Should_HaveInitialRequirement_As50Percent()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            const string expectedText = "Initial requirement · 50%";

            // act
            var profile = await dashboard.GoToProfile();

            // assert
            await Expect(profile.CurrentAndPastRequirements).ToBeVisibleAsync();
            var initialReq = profile.CurrentAndPastRequirements.First.Locator("div > strong");
            await Expect(initialReq).ToBeVisibleAsync();
            await Expect(initialReq).ToHaveTextAsync(expectedText);
        });
    }

    [Fact]
    public async Task Should_MakeEditFormVisible_WhenClickOnEdit()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();
            var initialEdit = profile.CurrentAndPastRequirements.First.Locator("div.actions > button");

            // act
            await initialEdit.ClickAsync();

            // assert
            var editForm = profile.EditAttendancesForm;
            await Expect(editForm).ToBeVisibleAsync();
        });
    }
    
    [Fact]
    public async Task Should_SetTextToRequiredOfficeDaysPercent_WhenPercentageIsSelected()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();
            var initialEdit = profile.CurrentAndPastRequirements.First.Locator("div.actions > button");
            await initialEdit.ClickAsync();
            var editForm = profile.EditAttendancesForm;
            var selectRequirementType = editForm.GetByLabel("Requirement type", new LocatorGetByLabelOptions { Exact = false });
            var inputProcDays = editForm.GetByLabel("Required office days (%)", new LocatorGetByLabelOptions { Exact = true });

            // act
            await selectRequirementType.SelectOptionAsync("Percentage");

            // assert
            await Expect(inputProcDays).ToBeVisibleAsync();
        });
    }
    
    [Fact]
    public async Task Should_SetTextToOfficeDaysPerWeek_WhenDaysPerWeekIsSelected()
    {
        await RunScenarioAsync(static async (page, seeder, ct) =>
        {
            // arrange
            var user = await seeder.CreateUserAsync(ct);
            var login = await page.GoTo<LoginPage>();
            var dashboard = await login.SignIn(user);
            var profile = await dashboard.GoToProfile();
            var initialEdit = profile.CurrentAndPastRequirements.First.Locator("div.actions > button");
            await initialEdit.ClickAsync();
            var editForm = profile.EditAttendancesForm;
            var selectRequirementType = editForm.GetByLabel("Requirement type", new LocatorGetByLabelOptions { Exact = false });
            var inputProcDays = editForm.GetByLabel("Office days per week", new LocatorGetByLabelOptions { Exact = true });

            // act
            await selectRequirementType.SelectOptionAsync("DaysPerWeek");

            // assert
            await Expect(inputProcDays).ToBeVisibleAsync();
        });
    }
}
