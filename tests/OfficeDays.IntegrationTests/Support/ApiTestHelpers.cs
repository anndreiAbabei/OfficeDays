using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeDays.Data;

namespace OfficeDays.IntegrationTests.Support;

internal static class ApiTestHelpers
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task CreateUser(HttpClient client, string username, string countryCode = "RO")
    {
        var response = await client.PostAsJsonAsync("/api/users", new
            { username, password = "Normal-user-password!", timeZoneId = "Europe/Bucharest", countryCode, email = $"{username}@example.com" });
        response.EnsureSuccessStatusCode();
    }

    public static async Task Login(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
    }

    public static async Task DeleteUser(OfficeDaysFactory factory, string username)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Users
                .Where(x => x.NormalizedUsername == username.ToUpperInvariant())
                .ExecuteDeleteAsync();
    }

    public static async Task<string> Csrf(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf")).GetProperty("token").GetString()!;

    public static Task<HttpResponseMessage> PostJson(HttpClient client, string path, object body, string csrf) => SendJson(client, HttpMethod.Post, path, body, csrf);
    public static Task<HttpResponseMessage> PutJson(HttpClient client, string path, object body, string csrf) => SendJson(client, HttpMethod.Put, path, body, csrf);
    public static Task<HttpResponseMessage> Delete(HttpClient client, string path, string csrf) => SendJson(client, HttpMethod.Delete, path, null, csrf);
    public static Task<HttpResponseMessage> SendJson(HttpClient client, HttpMethod method, string path, object? body, string csrf)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        if (body is not null) request.Content = JsonContent.Create(body);
        return client.SendAsync(request);
    }
    public static async Task<JsonElement> Read(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(), Json);
}
