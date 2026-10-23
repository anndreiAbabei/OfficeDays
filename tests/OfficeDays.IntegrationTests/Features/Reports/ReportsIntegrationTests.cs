using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Reports;

public sealed class ReportsIntegrationTests
{
    private static async Task<Guid> Register(HttpClient client, OfficeDaysFactory factory, string username)
    {
        await CreateUser(client, username);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = await db.Users.Where(x => x.Username == username).Select(x => x.Id).SingleAsync();
        await db.Users.Where(x => x.Id == id).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt,
            new DateTimeOffset(2099, 6, 15, 0, 0, 0, TimeSpan.Zero)));
        return id;
    }

    [Fact]
    public async Task Generates_private_monthly_report_with_history_time_off_and_future_targets()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        var userId = await Register(client, factory, "reportuser");
        var otherId = await Register(client, factory, "otheruser");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jurisdiction = (await db.Users.SingleAsync(x => x.Id == userId)).HolidayJurisdictionId;
            var monday = Enumerable.Range(1, 7).Select(day => new DateOnly(2099, 6, day)).First(day => day.DayOfWeek == DayOfWeek.Monday);
            db.BankHolidays.Add(new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdictionId = jurisdiction, Date = monday, Name = "Test holiday" });
            db.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = userId, From = monday, To = monday.AddDays(1) });
            db.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = otherId, From = new DateOnly(2099, 6, 1), To = new DateOnly(2099, 6, 30) });
            db.Attendances.AddRange(new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = userId, Date = monday },
                new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = userId, Date = monday.AddDays(2) },
                new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = otherId, Date = monday.AddDays(3) });
            db.UserAttendanceRequirements.AddRange(
                new UserAttendanceRequirement { Id = Guid.NewGuid(), UserId = userId, StartingFrom = new DateOnly(2099, 7, 1), RequirementValue = 100 },
                new UserAttendanceRequirement { Id = Guid.NewGuid(), UserId = userId, StartingFrom = new DateOnly(2099, 12, 1), RequirementValue = 60 });
            await db.SaveChangesAsync();
        }
        await Login(client, "reportuser", "Normal-user-password!");
        var response = await client.GetAsync("/api/reports");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var report = await Read(response);
        Assert.Equal("reportuser", report.GetProperty("username").GetString());
        Assert.Equal(2099, report.GetProperty("year").GetInt32());
        Assert.Equal(1, report.GetProperty("startMonth").GetInt32());
        Assert.Equal(12, report.GetProperty("endMonth").GetInt32());
        Assert.Equal("2099-06-01", report.GetProperty("registrationMonth").GetString());
        Assert.Equal("2099-09-15", report.GetProperty("today").GetString());
        Assert.NotEqual(default, report.GetProperty("generatedAt").GetDateTimeOffset());
        var rows = report.GetProperty("months").EnumerateArray().ToArray();
        Assert.Equal(Enumerable.Range(6, 7), rows.Select(x => x.GetProperty("month").GetInt32()));
        Assert.Equal(1, rows[0].GetProperty("officeDays").GetInt32());
        Assert.Equal(50, rows[0].GetProperty("requirementValue").GetInt32());
        Assert.Equal(100, rows[1].GetProperty("requirementValue").GetInt32());
        Assert.Equal("inProgress", rows[3].GetProperty("result").GetString());
        Assert.Equal("notStarted", rows[6].GetProperty("result").GetString());
        Assert.Equal(60, rows[6].GetProperty("requirementValue").GetInt32());
        Assert.Equal(JsonValueKind.Null, rows[6].GetProperty("officeDays").ValueKind);
        Assert.Equal(1, report.GetProperty("summary").GetProperty("officeDays").GetInt32());
        Assert.Equal(3, report.GetProperty("summary").GetProperty("completedMonths").GetInt32());
        foreach (var row in rows)
        {
            var status = await client.GetFromJsonAsync<JsonElement>($"/api/status?year=2099&month={row.GetProperty("month").GetInt32()}");
            Assert.Equal(status.GetProperty("eligibleWorkingDays").GetInt32(), row.GetProperty("eligibleWorkingDays").GetInt32());
            Assert.Equal(status.GetProperty("requiredOfficeDays").GetInt32(), row.GetProperty("requiredOfficeDays").GetInt32());
        }
        var other = await client.GetFromJsonAsync<JsonElement>($"/api/reports?year=2099&userId={otherId}");
        Assert.Equal("reportuser", other.GetProperty("username").GetString());
    }

    [Fact]
    public async Task Range_is_inclusive_and_refresh_reflects_corrections_without_storing_reports()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await Register(client, factory, "refreshuser");
        await Login(client, "refreshuser", "Normal-user-password!");
        var csrf = await Csrf(client);
        var before = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&startMonth=6&endMonth=6");
        Assert.Single(before.GetProperty("months").EnumerateArray());
        var day = Enumerable.Range(1, 7).Select(d => new DateOnly(2099, 6, d)).First(d => d.DayOfWeek == DayOfWeek.Monday);
        (await PutJson(client, $"/api/attendance/{day:yyyy-MM-dd}", new { }, csrf)).EnsureSuccessStatusCode();
        (await PostJson(client, "/api/users/me/requirements", new { startingFrom = "2099-06-01", requirementMode = "Percentage", requirementValue = 0 }, csrf)).EnsureSuccessStatusCode();
        var after = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&startMonth=6&endMonth=6");
        Assert.Equal(0, before.GetProperty("summary").GetProperty("officeDays").GetInt32());
        Assert.Equal(1, after.GetProperty("summary").GetProperty("officeDays").GetInt32());
        Assert.Equal(0, after.GetProperty("months")[0].GetProperty("requiredOfficeDays").GetInt32());
        Assert.Equal("targetMet", after.GetProperty("months")[0].GetProperty("result").GetString());
        var empty = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&endMonth=5");
        Assert.Empty(empty.GetProperty("months").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, empty.GetProperty("summary").GetProperty("attendancePercentage").ValueKind);
        var older = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2098");
        Assert.Empty(older.GetProperty("months").EnumerateArray());
    }

    [Fact]
    public async Task Should_RejectInvalidFiltersAndDeletedSessions_WhenRequestingReports()
    {
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/reports")).StatusCode);
        await Register(client, factory, "validationuser");
        await Login(client, "validationuser", "Normal-user-password!");
        foreach (var query in new[] { "year=0", "year=9999", "year=invalid", "startMonth=0", "endMonth=13", "startMonth=8&endMonth=7" })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/reports?{query}")).StatusCode);
        var expectedUnauthorized = HttpStatusCode.Unauthorized;
        await DeleteUser(factory, "validationuser");
        Assert.Equal(expectedUnauthorized, (await client.GetAsync("/api/reports")).StatusCode);
    }

    [Fact]
    public async Task Should_ShareOneTransaction_WhenReadingReportInputs()
    {
        var capture = new TransactionCapture();
        await using var original = new OfficeDaysFactory();
        await using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
                                                                                                       services.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(capture))));
        using var client = factory.CreateClient();
        await CreateUser(client, "snapshotuser");
        await Login(client, "snapshotuser", "Normal-user-password!");
        capture.Enabled = true;
        (await client.GetAsync("/api/reports")).EnsureSuccessStatusCode();
        // Authentication performs one existence query before the five report queries.
        const int expectedQueryCount = 6;
        Assert.Equal(expectedQueryCount, capture.Transactions.Count);
        Assert.Null(capture.Transactions[0]);
        Assert.NotNull(capture.Transactions[1]);
        Assert.All(capture.Transactions.Skip(1), transaction => Assert.Same(capture.Transactions[1], transaction));
    }

    private sealed class TransactionCapture : DbCommandInterceptor
    {
        public bool Enabled { get; set; }
        public List<DbTransaction?> Transactions { get; } = [];
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (Enabled) Transactions.Add(command.Transaction);
            return ValueTask.FromResult(result);
        }
    }
}
