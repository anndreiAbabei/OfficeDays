using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.E2ETests.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class PageExtensions
{
    extension(IPage page)
    {
        public async Task<TPage> GoTo<TPage>(PageGotoOptions? options = null)
            where TPage : IScenarioPage<TPage>
        {
            var url = TPage.Url;

            await page.GotoAsync(url, options);
        
            return TPage.Create(page);
        }
        
        public async Task<TPage> Expects<TPage>(PageAssertionsToHaveURLOptions? options = null)
            where TPage : IScenarioPage<TPage>
        {
            var url = TPage.Url;

            await Expect(page).ToHaveURLAsync(url, options);
        
            return TPage.Create(page);
        }
    }

}
