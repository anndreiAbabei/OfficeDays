using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class UsersIntegrationTests
{
    [Fact]
    public async Task User_creation_is_normal_and_cookie_login_authenticates()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            username = "andrew", password = "A-personal-password!", timeZoneId = "Europe/Bucharest",
            countryCode = "RO", email = "andrew@example.com", isAdmin = true
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await Read(response);
        Assert.False(created.GetProperty("isAdmin").GetBoolean());

        await Login(client, "andrew", "A-personal-password!");
        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal("andrew", me.GetProperty("username").GetString());
        Assert.False(me.GetProperty("isAdmin").GetBoolean());
    }

    [Fact]
    public async Task User_creation_rejects_invalid_input_and_duplicate_usernames()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();

        var invalid = await client.PostAsJsonAsync("/api/users", new
            { username = "x", password = "short", timeZoneId = "Not/AZone", countryCode = "RO", email = "invalid@example.com" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        await CreateUser(client, "CaseSensitiveName");
        var duplicate = await client.PostAsJsonAsync("/api/users", new
            { username = "casesensitivename", password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode = "RO", email = "duplicate@example.com" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var unknownJurisdiction = await client.PostAsJsonAsync("/api/users", new
            { username = "unknowncountry", password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode = "US", email = "unknowncountry@example.com" });
        Assert.Equal(HttpStatusCode.BadRequest, unknownJurisdiction.StatusCode);
    }

    [Theory]
    [InlineData("person@example.com")]
    [InlineData(" person@example.com ")]
    public async Task Email_can_be_created_updated_and_cleared(string email)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/users", new
        {
            username = "emailuser", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO", email
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(email.Trim(), (await Read(created)).GetProperty("email").GetString());
        await CreateUser(client, "otheruser");
        await Login(client, "emailuser", "Normal-user-password!");
        var csrf = await Csrf(client);
        var updated = await PutJson(client, "/api/users/me", new { email = " updated@example.com " }, csrf);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("updated@example.com", (await Read(updated)).GetProperty("email").GetString());
        Assert.Equal("updated@example.com", (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("email").GetString());
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal("otheruser@example.com", (await db.Users.SingleAsync(x => x.Username == "otheruser")).Email);
        }
        foreach (var empty in new string?[] { null, "", "   " })
        {
            (await PutJson(client, "/api/users/me", new { email = "reset@example.com" }, csrf)).EnsureSuccessStatusCode();
            var cleared = await PutJson(client, "/api/users/me", new { email = empty }, csrf);
            Assert.Equal(HttpStatusCode.OK, cleared.StatusCode);
            Assert.Equal(JsonValueKind.Null, (await Read(cleared)).GetProperty("email").ValueKind);
            Assert.Equal(JsonValueKind.Null, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("email").ValueKind);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task User_creation_accepts_empty_email_as_null(string? email)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            username = "missingemail", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO", email
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await Read(response)).GetProperty("email").ValueKind);
        using var scope = factory.Services.CreateScope();
        Assert.Null((await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Users.SingleAsync(user => user.Username == "missingemail")).Email);
    }

    [Fact]
    public async Task User_creation_and_update_accept_omitted_email()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/users", new
        {
            username = "omittedemail", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await Read(created)).GetProperty("email").ValueKind);

        await CreateUser(client, "existingemail");
        await Login(client, "existingemail", "Normal-user-password!");
        var updated = await PutJson(client, "/api/users/me", new { requiredOfficePercentage = 25 }, await Csrf(client));
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("email").ValueKind);
        Assert.Equal(25, profile.GetProperty("requiredOfficePercentage").GetInt32());
    }

    [Fact]
    public async Task Email_update_requires_authentication_csrf_and_valid_email()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PutAsJsonAsync("/api/users/me", new { email = "user@example.com" })).StatusCode);
        await CreateUser(client, "emailvalidation");
        await Login(client, "emailvalidation", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.PutAsJsonAsync("/api/users/me", new { email = "user@example.com" })).StatusCode);
        var csrf = await Csrf(client);
        foreach (var invalid in new[] { "invalid", "a@b@example.com", "@example.com", "person@" })
        {
            Assert.Equal(HttpStatusCode.BadRequest,
                (await PutJson(client, "/api/users/me", new { email = invalid }, csrf)).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/users", new
            {
                username = "invalidemail", password = "Normal-user-password!",
                timeZoneId = "Europe/Bucharest", countryCode = "RO", email = invalid
            })).StatusCode);
        }
        Assert.Equal("emailvalidation@example.com", (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("email").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task Office_percentage_is_persisted_and_updates_status(int percentage)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/users", new
        {
            username = "percentageuser", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO", email = "percentage@example.com", requiredOfficePercentage = percentage
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(percentage, (await Read(created)).GetProperty("requiredOfficePercentage").GetInt32());
        await CreateUser(client, "otherpercentage");
        await Login(client, "percentageuser", "Normal-user-password!");
        var csrf = await Csrf(client);
        var profile = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal(percentage, profile.GetProperty("requiredOfficePercentage").GetInt32());
        var status = await client.GetFromJsonAsync<JsonElement>("/api/status?year=2026&month=9");
        Assert.Equal((int)Math.Ceiling(status.GetProperty("eligibleWorkingDays").GetInt32() * percentage / 100m),
            status.GetProperty("requiredOfficeDays").GetInt32());

        var updated = await PutJson(client, "/api/users/me", new { email = "percentage@example.com", requiredOfficePercentage = 25 }, csrf);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal(25, (await Read(updated)).GetProperty("requiredOfficePercentage").GetInt32());
        // Older clients that only update email must preserve the percentage.
        (await PutJson(client, "/api/users/me", new { email = "user@example.com" }, csrf)).EnsureSuccessStatusCode();
        Assert.Equal(25, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("requiredOfficePercentage").GetInt32());
        status = await client.GetFromJsonAsync<JsonElement>("/api/status?year=2026&month=9");
        Assert.Equal((int)Math.Ceiling(status.GetProperty("eligibleWorkingDays").GetInt32() * .25m),
            status.GetProperty("requiredOfficeDays").GetInt32());
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(50, (await db.Users.SingleAsync(x => x.Username == "otherpercentage")).RequiredOfficePercentage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(50.5)]
    public async Task Invalid_office_percentages_are_rejected(decimal percentage)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/users", new
        {
            username = "invalidpercentage", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO", email = "percentage@example.com", requiredOfficePercentage = percentage
        })).StatusCode);
        await CreateUser(client, "percentagevalidation");
        await Login(client, "percentagevalidation", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, "/api/users/me",
            new { email = "percentagevalidation@example.com", requiredOfficePercentage = percentage }, await Csrf(client))).StatusCode);
        Assert.Equal(50, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("requiredOfficePercentage").GetInt32());
    }

}
