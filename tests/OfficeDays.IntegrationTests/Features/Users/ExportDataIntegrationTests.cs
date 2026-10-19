using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Reports.GetReport;
using OfficeDays.Features.Operations.GetVersion.Contracts;
using OfficeDays.Features.Users.ExportData.Contracts;
using OfficeDays.Services;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class ExportDataIntegrationTests
{
    [Fact]
    public async Task Export_and_version_endpoint_use_the_same_version_service()
    {
        using var original = new OfficeDaysFactory();
        using var factory = original.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IAppVersionService>(new TestAppVersionService())));
        using var client = factory.CreateClient();
        await CreateUser(client, "versionedexport");
        await Login(client, "versionedexport", "Normal-user-password!");

        var version = (await client.GetFromJsonAsync<GetVersionResponse>("/api/version"))!;
        var export = (await client.GetFromJsonAsync<PersonalDataExport>("/api/users/me/export"))!;

        Assert.Equal("9.8.7+test-build", version.Version);
        Assert.Equal(version.Version, export.Application.Version);
    }

    private sealed class TestAppVersionService : IAppVersionService
    {
        public string Version => "9.8.7+test-build";
    }

    [Fact]
    public async Task Export_requires_authentication_and_handles_deleted_accounts()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/me/export")).StatusCode);
        await CreateUser(client, "deleted");
        await Login(client, "deleted", "Normal-user-password!");
        await DeleteUser(factory, "deleted");
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/users/me/export")).StatusCode);
    }

    [Fact]
    public async Task Empty_account_exports_explicit_nulls_and_empty_collections()
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "empty");
        await Login(client, "empty", "Normal-user-password!");
        var export = (await client.GetFromJsonAsync<PersonalDataExport>("/api/users/me/export"))!;
        Assert.Empty(export.Attendance);
        Assert.Empty(export.Plans);
        Assert.Empty(export.Vacations);
        Assert.Empty(export.HolidayCalendar.Holidays);
        Assert.Null(export.Preferences.TeamDay);
        Assert.Equal(DateOnly.MinValue, Assert.Single(export.RequirementHistory).StartingFrom);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Export_preserves_all_inputs_excludes_other_users_and_secrets_and_recreates_report(bool isAdmin)
    {
        using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "exporter");
        await CreateUser(client, "other");
        Guid ownerId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = await db.Users.Where(x => x.Username == "exporter" || x.Username == "other").ToListAsync();
            var owner = users.Single(x => x.Username == "exporter");
            ownerId = owner.Id;
            db.Entry(owner).Property(x => x.IsAdmin).CurrentValue = isAdmin;
            foreach (var user in users)
            {
                db.Entry(user).Property(x => x.CreatedAt).CurrentValue = new DateTimeOffset(2098, 12, 31, 23, 30, 0, TimeSpan.Zero);
                user.Preferences.TeamDay = DayOfWeek.Thursday;
                db.Attendances.AddRange(new[] { new DateOnly(2099, 9, 1), new DateOnly(2099, 9, 2), new DateOnly(2100, 1, 1) }
                    .Select(date => new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = user.Id, Date = date, IsManual = true, CreatedAt = user.CreatedAt }));
                db.OfficePlans.AddRange(new[] { new DateOnly(2098, 1, 1), new DateOnly(2100, 1, 1) }
                    .Select(date => new OfficePlan { Id = Guid.NewGuid(), UserId = user.Id, Date = date, CreatedAt = user.CreatedAt }));
                db.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = user.Id, From = new DateOnly(2099, 8, 31), To = new DateOnly(2099, 9, 2), CreatedAt = user.CreatedAt });
                db.UserRequiredOfficePercentages.Add(new UserRequiredOfficePercentage { Id = Guid.NewGuid(), UserId = user.Id, StartingFrom = new DateOnly(2099, 10, 1), RequiredOfficePercentage = 75, CreatedAt = user.CreatedAt });
                db.ApiTokens.Add(new ApiToken { Id = Guid.NewGuid(), UserId = user.Id, Name = "secret-token-name", TokenHash = $"secret-token-hash-{user.Id}", CreatedAt = user.CreatedAt });
            }
            var jurisdiction = users[0].HolidayJurisdictionId;
            db.BankHolidays.AddRange(new[] { new DateOnly(2098, 1, 1), new DateOnly(2099, 9, 3), new DateOnly(2100, 1, 1) }
                .Select(date => new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdictionId = jurisdiction, Date = date, Name = "Shared holiday" }));
            var otherCalendar = new HolidayJurisdiction { Code = "XX", Name = "Unrelated calendar" };
            db.HolidayJurisdictions.Add(otherCalendar);
            await db.SaveChangesAsync();
            db.BankHolidays.Add(new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdictionId = otherCalendar.Id, Date = new DateOnly(2099, 1, 1), Name = "Unrelated holiday" });
            await db.SaveChangesAsync();
        }
        await Login(client, "exporter", "Normal-user-password!");
        var response = await client.GetAsync("/api/users/me/export?userId=ignored");
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Contains(".json", response.Content.Headers.ContentDisposition!.FileNameStar!);
        var raw = await response.Content.ReadAsStringAsync();
        foreach (var secret in new[] { "password", "token", "secret-token", "other@example.com", "Unrelated" })
            Assert.DoesNotContain(secret, raw, StringComparison.OrdinalIgnoreCase);
        var export = JsonSerializer.Deserialize<PersonalDataExport>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal("officedays-personal-data", export.Format);
        Assert.Equal(1, export.FormatVersion);
        Assert.Equal(typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, export.Application.Version);
        Assert.Equal(new DateTimeOffset(2099, 9, 15, 9, 0, 0, TimeSpan.Zero), export.ExportedAt);
        Assert.Equal(new DateOnly(2099, 1, 1), export.ReportContext.RegistrationLocalDate);
        Assert.Equal(new DateOnly(2099, 9, 15), export.ReportContext.LocalDate);
        Assert.Equal(ownerId, export.Profile.Id);
        Assert.Equal(isAdmin, export.Profile.IsAdmin);
        Assert.Equal("thursday", export.Preferences.TeamDay);
        Assert.Equal(3, export.Attendance.Length);
        Assert.Equal(2, export.Plans.Length);
        Assert.Single(export.Vacations);
        Assert.Equal(2, export.RequirementHistory.Length);
        Assert.Equal(3, export.HolidayCalendar.Holidays.Length);
        Assert.All(export.Attendance, x => Assert.Equal(ownerId, x.UserId));
        Assert.All(export.Plans, x => Assert.Equal(ownerId, x.UserId));
        Assert.All(export.Vacations, x => Assert.Equal(ownerId, x.UserId));
        Assert.All(export.RequirementHistory, x => Assert.Equal(ownerId, x.UserId));
        Assert.All(export.HolidayCalendar.Holidays, x => Assert.Equal(export.Profile.HolidayJurisdictionId, x.HolidayJurisdictionId));

        // Reconstruct exclusively from the downloaded document, without querying the source database.
        var reconstructedUser = new User { Id = export.Profile.Id, Username = export.Profile.Username,
            NormalizedUsername = export.Profile.NormalizedUsername, PasswordHash = "unused",
            TimeZoneId = export.Profile.TimeZoneId, CreatedAt = export.Profile.CreatedAt };
        using var services = factory.Services.CreateScope();
        var calculator = services.ServiceProvider.GetRequiredService<IReportCalculator>();
        var report = calculator.Calculate(2099, 1, 12, reconstructedUser, export.ExportedAt,
            export.RequirementHistory.Select(x => new UserRequiredOfficePercentage { Id = x.Id, UserId = x.UserId, StartingFrom = x.StartingFrom, RequiredOfficePercentage = x.RequiredOfficePercentage, CreatedAt = x.CreatedAt }),
            export.HolidayCalendar.Holidays.Select(x => x.Date), export.Vacations.Select(x => (x.From, x.To)), export.Attendance.Select(x => x.Date));
        var original = await client.GetFromJsonAsync<JsonElement>("/api/reports?year=2099&startMonth=1&endMonth=12");
        Assert.Equal(original.GetRawText(), JsonSerializer.Serialize(report.ToViewModel(), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
