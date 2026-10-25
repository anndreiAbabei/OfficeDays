using OfficeDays.E2ETests.Infrastructure;

namespace OfficeDays.E2ETests;

public static class RecordingProgram
{
    public static async Task<int> Main(string[] args)
    {
        if (args is ["install", ..])
            return Program.Main(args);
        
        if (args is not ["record"])
        {
            await Console.Error.WriteLineAsync("Usage: dotnet run --project tests/OfficeDays.E2ETests -- [record | install chromium]");
            return 1;
        }
        
        await using var application = new ApplicationFixture();
        await application.InitializeAsync();
        
        Console.WriteLine($"Recording against disposable OfficeDays at {application.BaseUrl}");
        Console.WriteLine($"Admin username: Admin; test password: {ApplicationFixture.AdminPassword}");
        Console.WriteLine("Use Record in the Inspector, select C#, then copy the code. Click Resume to finish and clean up.");
        
        await using var context = await application.Browser.CreateContextAsync(application.BaseUrl, headed: true);
        
        var page = await context.NewPageAsync();
        await page.GotoAsync("/login");
        await page.PauseAsync();
        
        return 0;
    }
}
