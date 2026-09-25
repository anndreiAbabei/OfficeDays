using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

namespace OfficeDays.IntegrationTests;

public sealed class ApiIntegrationTests
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(null, false)]
    [InlineData("{}", false)]
    [InlineData("{\"isManual\":false}", false)]
    [InlineData("{\"isManual\":true}", true)]
    public async Task Attendance_source_is_persisted_and_duplicates_preserve_it(string? body, bool isManual)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "sourceuser");
        await Login(client, "sourceuser", "Normal-user-password!");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await Csrf(client));
        using var content = body is null ? null : new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var created = await client.PostAsync("/api/attendance", content);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(isManual, (await Read(created)).GetProperty("isManual").GetBoolean());

        var duplicate = await client.PostAsJsonAsync("/api/attendance", new { isManual = !isManual });
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.Equal(isManual, (await Read(duplicate)).GetProperty("isManual").GetBoolean());
        var putDuplicate = await client.PutAsync("/api/attendance/2099-09-15", null);
        Assert.Equal(HttpStatusCode.OK, putDuplicate.StatusCode);
        Assert.Equal(isManual, (await Read(putDuplicate)).GetProperty("isManual").GetBoolean());

        var rows = await client.GetFromJsonAsync<JsonElement[]>("/api/attendance");
        Assert.Equal(isManual, Assert.Single(rows!).GetProperty("isManual").GetBoolean());
        using var scope = factory.Services.CreateScope();
        Assert.Equal(isManual, (await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Attendances.SingleAsync()).IsManual);
    }

    [Fact]
    public async Task Put_attendance_is_always_manual_and_post_preserves_it()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "putsource");
        await Login(client, "putsource", "Normal-user-password!");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await Csrf(client));
        var created = await client.PutAsJsonAsync("/api/attendance/2099-09-15", new { isManual = false });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.True((await Read(created)).GetProperty("isManual").GetBoolean());
        var duplicate = await client.PostAsync("/api/attendance", null);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.True((await Read(duplicate)).GetProperty("isManual").GetBoolean());
        var past = await client.PutAsync("/api/attendance/2099-09-14", null);
        Assert.Equal(HttpStatusCode.Created, past.StatusCode);
        Assert.True((await Read(past)).GetProperty("isManual").GetBoolean());
    }

    [Theory]
    [InlineData("{\"isManual\":\"yes\"}")]
    [InlineData("{\"isManual\":null}")]
    [InlineData("{")]
    public async Task Invalid_attendance_body_does_not_create_a_record(string body)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "invalidsource");
        await Login(client, "invalidsource", "Normal-user-password!");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await Csrf(client));
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/attendance", content)).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/attendance"))!);
    }

    [Fact]
    public async Task Login_cookie_is_persistent()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "persistentuser");

        var response = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "persistentuser", password = "Normal-user-password!" });

        response.EnsureSuccessStatusCode();
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith("OfficeDays.Session=", StringComparison.Ordinal));
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Bearer_tokens_are_rejected_over_http_when_https_is_required()
    {
        using var factory = new OfficeDaysFactory(requireHttpsForBearerTokens: true);
        using var browser = factory.CreateClient();
        await CreateUser(browser, "secureuser");
        await Login(browser, "secureuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var created = await Read(await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf));

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", created.GetProperty("token").GetString());
        var response = await shortcut.PostAsync("/api/attendance", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("HTTPS required", (await Read(response)).GetProperty("title").GetString());
    }

    [Fact]
    public async Task User_creation_is_normal_and_cookie_login_authenticates()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            username = "andrew", password = "A-personal-password!", timeZoneId = "Europe/Bucharest",
            countryCode = "RO", isAdmin = true
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
    public async Task Admin_can_replace_holidays_but_normal_user_cannot()
    {
        using var factory = new OfficeDaysFactory();
        using var normal = factory.CreateClient();
        await CreateUser(normal, "normal");
        await Login(normal, "normal", "Normal-user-password!");
        var normalCsrf = await Csrf(normal);
        var denied = await PutJson(normal, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-01-01", name = "New Year" } }, normalCsrf);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        var first = await PutJson(admin, "/api/bank-holidays/RO/2026", new[]
        {
            new { date = "2026-01-01", name = "New Year" }, new { date = "2026-12-25", name = "Christmas" }
        }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var second = await PutJson(admin, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-05-01", name = "Labour Day" } }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var holidays = await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026");
        Assert.Single(holidays!);
        Assert.Equal("2026-05-01", holidays![0].GetProperty("date").GetString());

        await PostJson(admin, "/api/holiday-jurisdictions",
            new { code = "GB-NIR", name = "Northern Ireland" }, adminCsrf);
        Assert.Equal(HttpStatusCode.OK,
            (await PutJson(admin, "/api/bank-holidays/GB-NIR/2026",
                new[] { new { date = "2026-05-01", name = "Northern Ireland holiday" } }, adminCsrf)).StatusCode);
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/GB-NIR/2026"))!);
        Assert.Single((await admin.GetFromJsonAsync<JsonElement[]>("/api/bank-holidays/RO/2026"))!);
    }

    [Fact]
    public async Task Token_authenticates_updates_last_use_and_revocation_is_immediate()
    {
        using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "tokenuser");
        await Login(browser, "tokenuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var createdResponse = await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await Read(createdResponse);
        var raw = created.GetProperty("token").GetString()!;
        Assert.StartsWith("odt_", raw);

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", raw);
        Assert.Equal(HttpStatusCode.Created, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
        var tokens = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens");
        Assert.NotEqual(JsonValueKind.Null, tokens![0].GetProperty("lastUsedAt").ValueKind);

        var id = created.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(browser, $"/api/tokens/{id}", csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await shortcut.PostAsync("/api/attendance", null)).StatusCode);
    }

    [Fact]
    public async Task Users_cannot_manage_other_users_tokens()
    {
        using var factory = new OfficeDaysFactory();
        using var first = factory.CreateClient();
        await CreateUser(first, "firstuser"); await Login(first, "firstuser", "Normal-user-password!");
        var firstCsrf = await Csrf(first);
        var token = await Read(await PostJson(first, "/api/tokens", new { name = "Mine" }, firstCsrf));

        using var second = factory.CreateClient();
        await CreateUser(second, "seconduser"); await Login(second, "seconduser", "Normal-user-password!");
        var secondCsrf = await Csrf(second);
        var response = await Delete(second, $"/api/tokens/{token.GetProperty("id").GetGuid()}", secondCsrf);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty((await second.GetFromJsonAsync<JsonElement[]>("/api/tokens"))!);
    }

    [Fact]
    public async Task Bearer_attendance_is_idempotent_and_can_be_removed()
    {
        using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "attendee"); await Login(browser, "attendee", "Normal-user-password!");
        var csrf = await Csrf(browser);
        var token = await Read(await PostJson(browser, "/api/tokens", new { name = "Shortcut" }, csrf));
        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.GetProperty("token").GetString());

        for (var i = 0; i < 10; i++) Assert.True((await shortcut.PostAsync("/api/attendance", null)).IsSuccessStatusCode);
        var rows = await shortcut.GetFromJsonAsync<JsonElement[]>("/api/attendance?year=2099&month=9");
        Assert.Single(rows!);
        Assert.Equal("2099-09-15", rows![0].GetProperty("date").GetString());
        Assert.False(rows[0].GetProperty("isManual").GetBoolean());
        Assert.Equal(HttpStatusCode.NoContent, (await shortcut.DeleteAsync("/api/attendance/2099-09-15")).StatusCode);
        Assert.Empty((await shortcut.GetFromJsonAsync<JsonElement[]>("/api/attendance?year=2099&month=9"))!);
    }

    [Fact]
    public async Task Vacation_creation_and_removal_are_scoped_to_the_user()
    {
        using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "vacationer"); await Login(owner, "vacationer", "Normal-user-password!");
        var csrf = await Csrf(owner);
        var created = await Read(await PostJson(owner, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf));

        using var other = factory.CreateClient();
        await CreateUser(other, "outsider"); await Login(other, "outsider", "Normal-user-password!");
        var otherCsrf = await Csrf(other);
        Assert.Equal(HttpStatusCode.NotFound, (await Delete(other, $"/api/vacations/{created.GetProperty("id").GetGuid()}", otherCsrf)).StatusCode);
        Assert.Single((await owner.GetFromJsonAsync<JsonElement[]>("/api/vacations?year=2026&month=9"))!);
        Assert.Equal(HttpStatusCode.NoContent, (await Delete(owner, $"/api/vacations/{created.GetProperty("id").GetGuid()}", csrf)).StatusCode);
    }

    [Fact]
    public async Task Status_uses_persisted_holidays_vacations_and_attendance()
    {
        using var factory = new OfficeDaysFactory();
        using var admin = factory.CreateClient();
        await Login(admin, "Admin", "VeryStrongAdminPassword!");
        var adminCsrf = await Csrf(admin);
        await PutJson(admin, "/api/bank-holidays/RO/2026", new[] { new { date = "2026-09-14", name = "Holiday" } }, adminCsrf);
        await PostJson(admin, "/api/holiday-jurisdictions",
            new { code = "GB-NIR", name = "Northern Ireland" }, adminCsrf);
        await PutJson(admin, "/api/bank-holidays/GB-NIR/2026",
            new[] { new { date = "2026-09-15", name = "Other jurisdiction holiday" } }, adminCsrf);

        using var user = factory.CreateClient();
        await CreateUser(user, "statususer"); await Login(user, "statususer", "Normal-user-password!");
        var csrf = await Csrf(user);
        await PostJson(user, "/api/vacations", new { from = "2026-09-21", to = "2026-09-25" }, csrf);
        await PutJson(user, "/api/attendance/2026-09-01", new { }, csrf);
        await PutJson(user, "/api/attendance/2026-09-02", new { }, csrf);
        await PutJson(user, "/api/attendance/2026-09-14", new { }, csrf); // holiday, does not count

        var status = await user.GetFromJsonAsync<JsonElement>("/api/status?year=2026&month=9");
        Assert.Equal(16, status.GetProperty("eligibleWorkingDays").GetInt32()); // 22 weekdays - 1 holiday - 5 vacation
        Assert.Equal(8, status.GetProperty("maximumWfhDays").GetInt32());
        Assert.Equal(8, status.GetProperty("requiredOfficeDays").GetInt32());
        Assert.Equal(2, status.GetProperty("officeDays").GetInt32());
        Assert.Equal(6, status.GetProperty("remainingOfficeDays").GetInt32());
    }

    [Fact]
    public async Task Status_defaults_to_the_users_current_month_and_rejects_incomplete_periods()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "statusdefaults");
        await Login(client, "statusdefaults", "Normal-user-password!");

        var current = await client.GetAsync("/api/status");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.Equal("2099-09", (await Read(current)).GetProperty("period").GetString());

        var incomplete = await client.GetAsync("/api/status?year=2099");
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        var invalid = await client.GetAsync("/api/status?year=2099&month=13");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        await DeleteUser(factory, "statusdefaults");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/status")).StatusCode);
    }

    [Fact]
    public async Task Login_rejects_unknown_users_and_wrong_passwords_and_logout_ends_the_session()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "loginbranches");

        var unknown = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "does-not-exist", password = "Normal-user-password!" });
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);

        var wrongPassword = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "loginbranches", password = "Incorrect-password!" });
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var missingCredentials = await client.PostAsJsonAsync("/api/auth/login", new
            { username = "", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, missingCredentials.StatusCode);

        await Login(client, "loginbranches", "Normal-user-password!");
        var csrf = await Csrf(client);
        var logout = await PostJson(client, "/api/auth/logout", new { }, csrf);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task User_creation_rejects_invalid_input_and_duplicate_usernames()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();

        var invalid = await client.PostAsJsonAsync("/api/users", new
            { username = "x", password = "short", timeZoneId = "Not/AZone" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        await CreateUser(client, "CaseSensitiveName");
        var duplicate = await client.PostAsJsonAsync("/api/users", new
            { username = "casesensitivename", password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode = "RO" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var unknownJurisdiction = await client.PostAsJsonAsync("/api/users", new
            { username = "unknowncountry", password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode = "US" });
        Assert.Equal(HttpStatusCode.BadRequest, unknownJurisdiction.StatusCode);
    }

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

    [Fact]
    public async Task Attendance_endpoints_reject_invalid_requests_and_report_missing_records()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "attendancefailures");
        await Login(client, "attendancefailures", "Normal-user-password!");
        var csrf = await Csrf(client);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/attendance/not-a-date", new { }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/attendance/2100-01-01", new { }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/attendance?year=2099")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/attendance?month=9")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await Delete(client, "/api/attendance/not-a-date", csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await Delete(client, "/api/attendance/2099-09-01", csrf)).StatusCode);

        await DeleteUser(factory, "attendancefailures");
        Assert.Equal(HttpStatusCode.NotFound,
            (await PostJson(client, "/api/attendance", new { }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await PutJson(client, "/api/attendance/2099-09-01", new { }, csrf)).StatusCode);
    }

    [Fact]
    public async Task Bank_holiday_endpoints_reject_invalid_years_and_replacement_payloads()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await Login(client, "Admin", "VeryStrongAdminPassword!");
        var csrf = await Csrf(client);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/bank-holidays/RO/0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026",
                new[] { new { date = "2025-12-31", name = "Wrong year" } }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026",
                new[] { new { date = "2026-01-01", name = "" } }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await PutJson(client, "/api/bank-holidays/RO/2026", new[]
            {
                new { date = "2026-01-01", name = "First" },
                new { date = "2026-01-01", name = "Duplicate" }
            }, csrf)).StatusCode);

        var nullPayload = new HttpRequestMessage(HttpMethod.Put, "/api/bank-holidays/RO/2026");
        nullPayload.Headers.Add("X-CSRF-TOKEN", csrf);
        nullPayload.Content = JsonContent.Create<List<object>?>(null);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(nullPayload)).StatusCode);
    }

    [Fact]
    public async Task Vacation_endpoints_reject_invalid_ranges_and_filters_and_report_missing_records()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "vacationfailures");
        await Login(client, "vacationfailures", "Normal-user-password!");
        var csrf = await Csrf(client);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await PostJson(client, "/api/vacations", new { from = "2026-09-25", to = "2026-09-21" }, csrf)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await client.GetAsync("/api/vacations?month=9")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await Delete(client, $"/api/vacations/{Guid.NewGuid()}", csrf)).StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" person@example.com ")]
    public async Task Email_can_be_created_updated_and_cleared(string? email)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/users", new
        {
            username = "emailuser", password = "Normal-user-password!",
            timeZoneId = "Europe/Bucharest", countryCode = "RO", email
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(email?.Trim(), (await Read(created)).GetProperty("email").GetString());
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
            Assert.Null((await db.Users.SingleAsync(x => x.Username == "otheruser")).Email);
        }
        foreach (var empty in new string?[] { null, "", "   " })
        {
            var cleared = await PutJson(client, "/api/users/me", new { email = empty }, csrf);
            Assert.Equal(HttpStatusCode.OK, cleared.StatusCode);
            Assert.Null((await Read(cleared)).GetProperty("email").GetString());
        }
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
        foreach (var invalid in new[] { "invalid", "a@b@example.com", new string('a', 243) + "@example.com" })
        {
            Assert.Equal(HttpStatusCode.BadRequest,
                (await PutJson(client, "/api/users/me", new { email = invalid }, csrf)).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/users", new
            {
                username = "invalidemail", password = "Normal-user-password!",
                timeZoneId = "Europe/Bucharest", countryCode = "RO", email = invalid
            })).StatusCode);
        }
        Assert.Null((await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("email").GetString());
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
            timeZoneId = "Europe/Bucharest", countryCode = "RO", requiredOfficePercentage = percentage
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

        var updated = await PutJson(client, "/api/users/me", new { requiredOfficePercentage = 25 }, csrf);
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
            timeZoneId = "Europe/Bucharest", countryCode = "RO", requiredOfficePercentage = percentage
        })).StatusCode);
        await CreateUser(client, "percentagevalidation");
        await Login(client, "percentagevalidation", "Normal-user-password!");
        Assert.Equal(HttpStatusCode.BadRequest, (await PutJson(client, "/api/users/me",
            new { requiredOfficePercentage = percentage }, await Csrf(client))).StatusCode);
        Assert.Equal(50, (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("requiredOfficePercentage").GetInt32());
    }

    private static async Task CreateUser(HttpClient client, string username, string countryCode = "RO")
    {
        var response = await client.PostAsJsonAsync("/api/users", new
            { username, password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode });
        response.EnsureSuccessStatusCode();
    }

    private static async Task Login(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
    }

    private static async Task DeleteUser(OfficeDaysFactory factory, string username)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Users
                .Where(x => x.NormalizedUsername == username.ToUpperInvariant())
                .ExecuteDeleteAsync();
    }

    private static async Task<string> Csrf(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf")).GetProperty("token").GetString()!;

    private static Task<HttpResponseMessage> PostJson(HttpClient client, string path, object body, string csrf) => SendJson(client, HttpMethod.Post, path, body, csrf);
    private static Task<HttpResponseMessage> PutJson(HttpClient client, string path, object body, string csrf) => SendJson(client, HttpMethod.Put, path, body, csrf);
    private static Task<HttpResponseMessage> Delete(HttpClient client, string path, string csrf) => SendJson(client, HttpMethod.Delete, path, null, csrf);
    private static Task<HttpResponseMessage> SendJson(HttpClient client, HttpMethod method, string path, object? body, string csrf)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        if (body is not null) request.Content = JsonContent.Create(body);
        return client.SendAsync(request);
    }
    private static async Task<JsonElement> Read(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(), Json);
}
