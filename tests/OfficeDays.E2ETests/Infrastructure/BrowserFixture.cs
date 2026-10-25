namespace OfficeDays.E2ETests.Infrastructure;

public sealed class BrowserFixture : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<IBrowserContext> CreateContextAsync(string baseUrl, bool headed = false)
    {
        _playwright ??= await Playwright.CreateAsync();
        _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !headed
        });
        return await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = baseUrl,
            TimezoneId = "UTC",
            Locale = "en-US",
            ReducedMotion = ReducedMotion.Reduce,
            ViewportSize = new ViewportSize { Width = 1440, Height = 1000 }
        });
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
