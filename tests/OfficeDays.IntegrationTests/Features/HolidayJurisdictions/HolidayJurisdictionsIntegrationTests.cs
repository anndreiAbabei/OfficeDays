using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.HolidayJurisdictions;

public sealed class HolidayJurisdictionsIntegrationTests
{
    [Fact]
    public async Task Holiday_jurisdictions_are_publicly_readable_and_admin_managed()
    {
        using var factory = new OfficeDaysFactory();
        using var anonymous = factory.CreateClient();

        var initial = await anonymous.GetFromJsonAsync<JsonElement[]>("/api/holiday-jurisdictions");
        Assert.Contains(initial!, item => item.GetProperty("code").GetString() == "RO");
        Assert.Equal(HttpStatusCode.OK,
            (await anonymous.GetAsync("/api/bank-holidays/RO/2026")).StatusCode);

        using var normal = factory.CreateClient();
        await CreateUser(normal, "jurisdictionnormal");
        await Login(normal, "jurisdictionnormal", "Normal-user-password!");
        var normalCsrf = await Csrf(normal);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await PostJson(normal, "/api/holiday-jurisdictions",
                new { code = "GB-NIR", name = "Northern Ireland" }, normalCsrf)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await PutJson(normal, "/api/holiday-jurisdictions/RO",
                new { name = "Changed" }, normalCsrf)).StatusCode);

        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        var created = await PostJson(admin, "/api/holiday-jurisdictions",
            new { code = "uk-nir", name = "Northern Ireland" }, adminCsrf);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal("GB-NIR", (await Read(created)).GetProperty("code").GetString());

        var updated = await PutJson(admin, "/api/holiday-jurisdictions/GB-NIR",
            new { name = "Northern Ireland calendar" }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        await CreateUser(anonymous, "northernirelanduser", "GB-NIR");
        await Login(anonymous, "northernirelanduser", "Normal-user-password!");
        var profile = await anonymous.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal("GB-NIR", profile.GetProperty("countryCode").GetString());
        Assert.Equal(HttpStatusCode.Conflict,
            (await Delete(admin, "/api/holiday-jurisdictions/GB-NIR", adminCsrf)).StatusCode);

        Assert.Equal(HttpStatusCode.Created,
            (await PostJson(admin, "/api/holiday-jurisdictions",
                new { code = "US-CA", name = "California" }, adminCsrf)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await Delete(admin, "/api/holiday-jurisdictions/US-CA", adminCsrf)).StatusCode);
    }

}
