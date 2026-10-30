using System.Diagnostics.CodeAnalysis;

namespace OfficeDays.E2ETests.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class BrowserFixture : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<IBrowserContext> CreateContextAsync(string baseUrl, bool headed = false)
    {
        _playwright ??= await Playwright.CreateAsync();
        var options = new BrowserTypeLaunchOptions
        {
            Headless = !headed
        };
        _browser ??= await _playwright.Chromium.LaunchAsync(options);
        var ctxOptions = new BrowserNewContextOptions
        {
            BaseURL = baseUrl,
            TimezoneId = "UTC",
            Locale = "en-US",
            ReducedMotion = ReducedMotion.Reduce,
            ViewportSize = new ViewportSize { Width = 1440, Height = 1000 }
        };
        
        return await _browser.NewContextAsync(ctxOptions);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
            await _browser.CloseAsync();
        
        _browser = null;
        
        _playwright?.Dispose();
        _playwright = null;
    }
}
