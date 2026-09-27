using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Attendance;

public sealed class AttendanceIntegrationTests
{
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

}
