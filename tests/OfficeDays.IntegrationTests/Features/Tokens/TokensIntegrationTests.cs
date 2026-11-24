using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;
using static OfficeDays.IntegrationTests.Support.ApiTestHelpers;

namespace OfficeDays.IntegrationTests.Features.Tokens;

public sealed class TokensIntegrationTests
{
    [Fact]
    public async Task Should_RejectBearerToken_WhenHttpIsUsedAndHttpsIsRequired()
    {
        // Arrange
        const HttpStatusCode expectedBadRequest = HttpStatusCode.BadRequest;
        const string expectedTitle = "HTTPS required";
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory(requireHttpsForBearerTokens: true);
        using var browser = factory.CreateClient();
        await CreateUser(browser, "secureuser");
        await Login(browser, "secureuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        using var creation = await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf);
        var created = await Read(creation);

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", created.GetProperty("token").GetString());

        // Act
        using var response = await shortcut.PostAsync("/api/attendance", null, cancellationToken);

        // Assert
        Assert.Equal(expectedBadRequest, response.StatusCode);
        Assert.Equal(expectedTitle, (await Read(response)).GetProperty("title").GetString());
    }

    [Fact]
    public async Task Should_RecordLastUseAndRejectFurtherRequests_WhenTokenIsUsedThenRevoked()
    {
        // Arrange
        const HttpStatusCode expectedCreated = HttpStatusCode.Created;
        const HttpStatusCode expectedNoContent = HttpStatusCode.NoContent;
        const HttpStatusCode expectedUnauthorized = HttpStatusCode.Unauthorized;
        const string expectedPurpose = "ios-shortcuts";
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "tokenuser");
        await Login(browser, "tokenuser", "Normal-user-password!");
        var csrf = await Csrf(browser);

        // Act
        using var createdResponse = await PostJson(browser, "/api/tokens", new { name = "Phone", purpose = expectedPurpose }, csrf);
        var created = await Read(createdResponse);
        var raw = created.GetProperty("token").GetString();

        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", raw);
        using var attendance = await shortcut.PostAsync("/api/attendance", null, cancellationToken);
        var tokens = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);

        var id = created.GetProperty("id").GetGuid();
        using var revocation = await Delete(browser, $"/api/tokens/{id}", csrf);
        using var deniedAttendance = await shortcut.PostAsync("/api/attendance", null, cancellationToken);

        // Assert
        Assert.Equal(expectedCreated, createdResponse.StatusCode);
        Assert.StartsWith("odt_", raw);
        Assert.Equal(expectedCreated, attendance.StatusCode);
        Assert.NotNull(tokens);
        var listed = Assert.Single(tokens);
        Assert.NotEqual(JsonValueKind.Null, listed.GetProperty("lastUsedAt").ValueKind);
        Assert.Equal(expectedPurpose, listed.GetProperty("purpose").GetString());
        Assert.False(listed.TryGetProperty("token", out _));
        Assert.Equal(expectedNoContent, revocation.StatusCode);
        Assert.Equal(expectedUnauthorized, deniedAttendance.StatusCode);
    }

    [Fact]
    public async Task Should_HideToken_WhenAnotherUserListsOrRevokesIt()
    {
        // Arrange
        const HttpStatusCode expectedNotFound = HttpStatusCode.NotFound;
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var first = factory.CreateClient();
        await CreateUser(first, "firstuser");
        await Login(first, "firstuser", "Normal-user-password!");
        var firstCsrf = await Csrf(first);
        using var creation = await PostJson(first, "/api/tokens", new { name = "Mine" }, firstCsrf);
        var token = await Read(creation);

        using var second = factory.CreateClient();
        await CreateUser(second, "seconduser");
        await Login(second, "seconduser", "Normal-user-password!");
        var secondCsrf = await Csrf(second);
        var tokenId = token.GetProperty("id").GetGuid();

        // Act
        using var response = await Delete(second, $"/api/tokens/{tokenId}", secondCsrf);
        var listed = await second.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);

        // Assert
        Assert.Equal(expectedNotFound, response.StatusCode);
        Assert.NotNull(listed);
        Assert.Empty(listed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_TrimNameAndPreserveCredentialAndMetadata_WhenTokenIsRenamed(bool revoked)
    {
        // Arrange
        const HttpStatusCode expectedCreated = HttpStatusCode.Created;
        const HttpStatusCode expectedNoContent = HttpStatusCode.NoContent;
        const string expectedName = "Work phone";
        var expectedCredentialStatus = revoked ? HttpStatusCode.Unauthorized : HttpStatusCode.OK;
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "renameuser");
        await Login(browser, "renameuser", "Normal-user-password!");
        var csrf = await Csrf(browser);
        using var creation = await PostJson(browser, "/api/tokens", new { name = "Phone" }, csrf);
        var created = await Read(creation);
        var id = created.GetProperty("id").GetGuid();
        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", created.GetProperty("token").GetString());
        using var attendance = await shortcut.PostAsync("/api/attendance", null, cancellationToken);
        Assert.Equal(expectedCreated, attendance.StatusCode);
        if (revoked)
        {
            using var revocation = await Delete(browser, $"/api/tokens/{id}", csrf);
            Assert.Equal(expectedNoContent, revocation.StatusCode);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var before = await db.ApiTokens.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);

        // Act
        using var response = await SendJson(browser, HttpMethod.Patch, $"/api/tokens/{id}", new { name = "  Work phone  " }, csrf);

        var after = await db.ApiTokens.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
        var listed = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);
        using var credentialResponse = await shortcut.PostAsync("/api/attendance", null, cancellationToken);

        // Assert
        Assert.Equal(expectedNoContent, response.StatusCode);
        Assert.Equal(expectedName, after.Name);
        Assert.Equal(before.TokenHash, after.TokenHash);
        Assert.Equal(before.UserId, after.UserId);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.Equal(before.LastUsedAt, after.LastUsedAt);
        Assert.Equal(before.RevokedAt, after.RevokedAt);
        Assert.NotNull(listed);
        Assert.Equal(expectedName, Assert.Single(listed).GetProperty("name").GetString());
        Assert.Equal(expectedCredentialStatus, credentialResponse.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-long")]
    public async Task Should_KeepOriginalName_WhenNewTokenNameIsInvalid(string? name)
    {
        // Arrange
        const HttpStatusCode expectedBadRequest = HttpStatusCode.BadRequest;
        const string expectedName = "Original";
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var browser = factory.CreateClient();
        await CreateUser(browser, "invalidrename");
        await Login(browser, "invalidrename", "Normal-user-password!");
        var csrf = await Csrf(browser);
        using var creation = await PostJson(browser, "/api/tokens", new { name = expectedName }, csrf);
        var created = await Read(creation);
        var id = created.GetProperty("id").GetGuid();
        if (name == "too-long")
            name = new string('x', 101);

        // Act
        using var response = await SendJson(browser, HttpMethod.Patch, $"/api/tokens/{id}", new { name }, csrf);
        var listed = await browser.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);

        // Assert
        Assert.Equal(expectedBadRequest, response.StatusCode);
        Assert.NotNull(listed);
        Assert.Equal(expectedName, Assert.Single(listed).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Should_AllowRenameOnlyForExistingOwnedToken_WhenAntiforgeryCheckPasses()
    {
        // Arrange
        const HttpStatusCode expectedUnauthorized = HttpStatusCode.Unauthorized;
        const HttpStatusCode expectedNotFound = HttpStatusCode.NotFound;
        const HttpStatusCode expectedBadRequest = HttpStatusCode.BadRequest;
        const HttpStatusCode expectedNoContent = HttpStatusCode.NoContent;
        const string expectedName = "Original";
        var expectedMaximumLengthName = new string('x', 100);
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "renameowner");
        await Login(owner, "renameowner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        using var creation = await PostJson(owner, "/api/tokens", new { name = expectedName }, csrf);
        var created = await Read(creation);
        var path = $"/api/tokens/{created.GetProperty("id").GetGuid()}";
        using var other = factory.CreateClient();

        // Act
        using var anonymousRename = await other.PatchAsJsonAsync(path, new { name = "Stolen" }, cancellationToken);
        await CreateUser(other, "renameother");
        await Login(other, "renameother", "Normal-user-password!");
        using var otherUserRename = await SendJson(other, HttpMethod.Patch, path, new { name = "Stolen" }, await Csrf(other));
        using var missingCsrfRename = await owner.PatchAsJsonAsync(path, new { name = "No CSRF" }, cancellationToken);
        using var missingTokenRename = await SendJson(owner, HttpMethod.Patch, $"/api/tokens/{Guid.NewGuid()}", new { name = "Missing" }, csrf);
        var unchanged = await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);
        using var ownerRename = await SendJson(owner, HttpMethod.Patch, path, new { name = expectedMaximumLengthName }, csrf);
        var listed = await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);

        // Assert
        Assert.Equal(expectedUnauthorized, anonymousRename.StatusCode);
        Assert.Equal(expectedNotFound, otherUserRename.StatusCode);
        Assert.Equal(expectedBadRequest, missingCsrfRename.StatusCode);
        Assert.Equal(expectedNotFound, missingTokenRename.StatusCode);
        Assert.NotNull(unchanged);
        Assert.Equal(expectedName, Assert.Single(unchanged).GetProperty("name").GetString());
        Assert.Equal(expectedNoContent, ownerRename.StatusCode);
        Assert.NotNull(listed);
        Assert.Equal(expectedMaximumLengthName, Assert.Single(listed).GetProperty("name").GetString());
    }

    [Fact]
    public async Task Should_AllowPermanentDeletionOnlyAfterRevocation_WhenRequestedByOwner()
    {
        // Arrange
        const HttpStatusCode expectedConflict = HttpStatusCode.Conflict;
        const HttpStatusCode expectedNoContent = HttpStatusCode.NoContent;
        const HttpStatusCode expectedBadRequest = HttpStatusCode.BadRequest;
        const HttpStatusCode expectedUnauthorized = HttpStatusCode.Unauthorized;
        const HttpStatusCode expectedNotFound = HttpStatusCode.NotFound;
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new OfficeDaysFactory();
        using var owner = factory.CreateClient();
        await CreateUser(owner, "deleteowner");
        await Login(owner, "deleteowner", "Normal-user-password!");
        var csrf = await Csrf(owner);
        using var creation = await PostJson(owner, "/api/tokens", new { name = "Phone" }, csrf);
        var created = await Read(creation);
        var id = created.GetProperty("id").GetGuid();
        var path = $"/api/tokens/{id}";

        var permanentPath = path + "/permanent";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var matchingTokens = db.ApiTokens.AsNoTracking().Where(token => token.Id == id);

        // Act
        using var activeDeletion = await Delete(owner, permanentPath, csrf);
        var active = await matchingTokens.SingleAsync(cancellationToken);

        using var revocation = await Delete(owner, path, csrf);
        using var missingCsrfDeletion = await owner.DeleteAsync(permanentPath, cancellationToken);

        using var other = factory.CreateClient();
        using var anonymousDeletion = await other.DeleteAsync(permanentPath, cancellationToken);
        await CreateUser(other, "deleteother");
        await Login(other, "deleteother", "Normal-user-password!");
        using var otherUserDeletion = await Delete(other, permanentPath, await Csrf(other));
        var existsAfterRejectedDeletion = await matchingTokens.AnyAsync(cancellationToken);

        using var ownerDeletion = await Delete(owner, permanentPath, csrf);
        var existsAfterOwnerDeletion = await matchingTokens.AnyAsync(cancellationToken);

        var listed = await owner.GetFromJsonAsync<JsonElement[]>("/api/tokens", cancellationToken);
        using var repeatedDeletion = await Delete(owner, permanentPath, csrf);
        using var shortcut = factory.CreateClient();
        shortcut.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", created.GetProperty("token").GetString());
        using var deniedAttendance = await shortcut.PostAsync("/api/attendance", null, cancellationToken);

        // Assert
        Assert.Equal(expectedConflict, activeDeletion.StatusCode);
        Assert.Equal(expectedNoContent, revocation.StatusCode);
        Assert.Equal(expectedBadRequest, missingCsrfDeletion.StatusCode);
        Assert.Equal(expectedUnauthorized, anonymousDeletion.StatusCode);
        Assert.Equal(expectedNotFound, otherUserDeletion.StatusCode);
        Assert.Equal(expectedNoContent, ownerDeletion.StatusCode);
        Assert.NotNull(listed);
        Assert.Empty(listed);
        Assert.Equal(expectedNotFound, repeatedDeletion.StatusCode);
        Assert.Equal(expectedUnauthorized, deniedAttendance.StatusCode);
        Assert.Null(active.RevokedAt);
        Assert.True(existsAfterRejectedDeletion);
        Assert.False(existsAfterOwnerDeletion);
    }
}
