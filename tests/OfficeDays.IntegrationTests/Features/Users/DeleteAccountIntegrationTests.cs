using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Security;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Users;

public sealed class DeleteAccountIntegrationTests
{
    private const string Password = "Normal-user-password!";

    [Fact]
    public async Task Should_DeleteOnlyOwnedDataAndInvalidateCredentials_WhenConfirmed()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        using var oldSession = factory.CreateClient();
        await CreateUser(client, "deleting");
        await CreateUser(client, "survivor");
        await Login(client, "deleting", Password);
        await Login(oldSession, "deleting", Password);
        var csrf = await Csrf(client);
        Guid deletedId;
        string token;
        int holidayCount;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.SingleAsync(x => x.Username == "deleting", cancellationToken: TestContext.Current.CancellationToken);
            deletedId = user.Id;
            var survivor = await db.Users.SingleAsync(x => x.Username == "survivor", cancellationToken: TestContext.Current.CancellationToken);
            var tokens = scope.ServiceProvider.GetRequiredService<IApiTokenService>();
            token = tokens.Generate();
            foreach (var owner in new[] { user, survivor })
            {
                db.Attendances.Add(new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = owner.Id, Date = new DateOnly(2099, 9, 14) });
                db.OfficePlans.Add(new OfficePlan { Id = Guid.NewGuid(), UserId = owner.Id, Date = new DateOnly(2099, 9, 16) });
                db.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = owner.Id, From = new DateOnly(2099, 10, 1), To = new DateOnly(2099, 10, 2) });
            }
            db.BankHolidays.Add(new BankHoliday { Id = Guid.NewGuid(), HolidayJurisdictionId = user.HolidayJurisdictionId, Date = new DateOnly(2099, 12, 25), Name = "Shared holiday" });
            db.ApiTokens.Add(new ApiToken { Id = Guid.NewGuid(), UserId = survivor.Id, Name = "survivor", TokenHash = tokens.Hash(tokens.Generate()) });
            db.ApiTokens.Add(new ApiToken { Id = Guid.NewGuid(), UserId = user.Id, Name = "integration", TokenHash = tokens.Hash(token) });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            holidayCount = await db.BankHolidays.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
        }

        // Act: a caller-supplied target must never select another user.
        var response = await SendJson(client, HttpMethod.Delete, "/api/users/me", new { password = Password, username = "survivor", userId = Guid.NewGuid() }, csrf);

        // Assert
        var expectedStatus = HttpStatusCode.NoContent;
        Assert.Equal(expectedStatus, response.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.False(await db.Users.AnyAsync(x => x.Id == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(await db.Attendances.AnyAsync(x => x.UserId == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(await db.OfficePlans.AnyAsync(x => x.UserId == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(await db.Vacations.AnyAsync(x => x.UserId == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(await db.UserAttendanceRequirements.AnyAsync(x => x.UserId == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.False(await db.ApiTokens.AnyAsync(x => x.UserId == deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Single(await db.Attendances.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.Single(await db.OfficePlans.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.Single(await db.Vacations.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.Single(await db.ApiTokens.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.True(await db.UserAttendanceRequirements.AnyAsync(x => x.UserId != deletedId, cancellationToken: TestContext.Current.CancellationToken));
            Assert.True(await db.Users.AnyAsync(x => x.Username == "survivor", cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(holidayCount, await db.BankHolidays.CountAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.NotEmpty(await db.HolidayJurisdictions.ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
        }
        var expectedUnauthorized = HttpStatusCode.Unauthorized;
        Assert.Equal(expectedUnauthorized, (await oldSession.GetAsync("/api/users/me/requirements", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedUnauthorized, (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedUnauthorized, (await SendJson(client, HttpMethod.Delete, "/api/users/me", new { password = Password }, csrf)).StatusCode);
        using var bearer = factory.CreateClient();
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Assert.Equal(expectedUnauthorized, (await bearer.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(expectedUnauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { username = "deleting", password = Password }, cancellationToken: TestContext.Current.CancellationToken)).StatusCode);
    }

    [Theory]
    [InlineData("wrong", true)]
    [InlineData("", true)]
    [InlineData(Password, false)]
    public async Task Should_PreserveAccount_WhenPasswordOrCsrfIsInvalid(string password, bool includeCsrf)
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "deleting");
        await Login(client, "deleting", Password);
        var csrf = includeCsrf ? await Csrf(client) : "invalid";

        // Act
        var response = await SendJson(client, HttpMethod.Delete, "/api/users/me", new { password }, csrf);

        // Assert
        var expectedStatus = HttpStatusCode.BadRequest;
        Assert.Equal(expectedStatus, response.StatusCode);
        (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Should_RejectTokenAndAnonymousRequests_WhenDeletingAccount()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "deleting");
        await Login(client, "deleting", Password);
        var csrf = await Csrf(client);
        var created = await PostJson(client, "/api/tokens", new { name = "test" }, csrf);
        created.EnsureSuccessStatusCode();
        var token = (await Read(created)).GetProperty("token").GetString();
        using var bearer = factory.CreateClient();
        bearer.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var anonymous = factory.CreateClient();

        // Act
        var forbidden = await SendJson(bearer, HttpMethod.Delete, "/api/users/me", new { password = Password }, csrf);
        var unauthorized = await SendJson(anonymous, HttpMethod.Delete, "/api/users/me", new { password = Password }, csrf);

        // Assert
        var expectedForbidden = HttpStatusCode.Forbidden;
        var expectedUnauthorized = HttpStatusCode.Unauthorized;
        Assert.Equal(expectedForbidden, forbidden.StatusCode);
        Assert.Equal(expectedUnauthorized, unauthorized.StatusCode);
        (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Should_BlockLastAdministrator_WhenDeletionWouldOrphanAdministration()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await Login(client, "Admin", "VeryStrongAdminPassword!");
        var csrf = await Csrf(client);

        // Act
        var response = await SendJson(client, HttpMethod.Delete, "/api/users/me", new { password = "VeryStrongAdminPassword!" }, csrf);

        // Assert
        var expectedStatus = HttpStatusCode.Conflict;
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Contains("another administrator", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Should_KeepOneAdministrator_WhenAdministratorsDeleteConcurrently()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var first = factory.CreateClient();
        using var second = factory.CreateClient();
        await CreateUser(second, "secondadmin");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Users.Where(x => x.Username == "secondadmin").ExecuteUpdateAsync(update => update.SetProperty(x => x.IsAdmin, true), cancellationToken: TestContext.Current.CancellationToken);
        }
        await Login(first, "Admin", "VeryStrongAdminPassword!");
        await Login(second, "secondadmin", Password);
        var firstCsrf = await Csrf(first);
        var secondCsrf = await Csrf(second);

        // Act
        var responses = await Task.WhenAll(
            SendJson(first, HttpMethod.Delete, "/api/users/me", new { password = "VeryStrongAdminPassword!" }, firstCsrf),
            SendJson(second, HttpMethod.Delete, "/api/users/me", new { password = Password }, secondCsrf));

        // Assert
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.NoContent);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Single(await verificationDb.Users.Where(x => x.IsAdmin).ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Should_RollBackAllDeletion_WhenDatabaseRejectsCascade()
    {
        // Arrange
        await using var factory = new OfficeDaysFactory();
        using var client = factory.CreateClient();
        await CreateUser(client, "deleting");
        await Login(client, "deleting", Password);
        var csrf = await Csrf(client);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.SingleAsync(x => x.Username == "deleting", cancellationToken: TestContext.Current.CancellationToken);
            db.Attendances.Add(new OfficeDays.Domain.Attendance { Id = Guid.NewGuid(), UserId = user.Id, Date = new DateOnly(2099, 9, 14) });
            db.OfficePlans.Add(new OfficePlan { Id = Guid.NewGuid(), UserId = user.Id, Date = new DateOnly(2099, 9, 16) });
            db.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = user.Id, From = new DateOnly(2099, 10, 1), To = new DateOnly(2099, 10, 2) });
            var tokens = scope.ServiceProvider.GetRequiredService<IApiTokenService>();
            db.ApiTokens.Add(new ApiToken { Id = Guid.NewGuid(), UserId = user.Id, Name = "rollback", TokenHash = tokens.Hash(tokens.Generate()) });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
            await db.Database.ExecuteSqlRawAsync("CREATE TRIGGER FailDeletion BEFORE DELETE ON UserAttendanceRequirements BEGIN SELECT RAISE(ABORT, 'test failure'); END;", cancellationToken: TestContext.Current.CancellationToken);
        }

        // Act
        var response = await SendJson(client, HttpMethod.Delete, "/api/users/me", new { password = Password }, csrf);

        // Assert
        var expectedStatus = HttpStatusCode.InternalServerError;
        Assert.Equal(expectedStatus, response.StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var survivingUser = await verificationDb.Users.SingleAsync(x => x.Username == "deleting", cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(await verificationDb.UserAttendanceRequirements.AnyAsync(x => x.UserId == survivingUser.Id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(await verificationDb.Attendances.AnyAsync(x => x.UserId == survivingUser.Id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(await verificationDb.OfficePlans.AnyAsync(x => x.UserId == survivingUser.Id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(await verificationDb.Vacations.AnyAsync(x => x.UserId == survivingUser.Id, cancellationToken: TestContext.Current.CancellationToken));
        Assert.True(await verificationDb.ApiTokens.AnyAsync(x => x.UserId == survivingUser.Id, cancellationToken: TestContext.Current.CancellationToken));
        (await client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
    }
}
