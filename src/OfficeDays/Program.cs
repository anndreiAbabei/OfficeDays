using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Endpoints;
using OfficeDays.Middleware;
using OfficeDays.Security;
using OfficeDays.Services;

var builder = WebApplication.CreateBuilder(args);
var cookieExpireTimeSpan = builder.Configuration.GetValue<TimeSpan>("Authentication:CookieExpireTimeSpan");
if (cookieExpireTimeSpan <= TimeSpan.Zero)
    throw new InvalidOperationException("Authentication:CookieExpireTimeSpan must be a positive TimeSpan.");

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<OfficeDays.Features.Operations.DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(5));
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(keysPath))
    builder.Services.AddDataProtection();
else
{
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "OfficeDays.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=officedays.db"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<UserDateService>();
builder.Services.AddScoped<CookieAntiforgeryFilter>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Smart";
        options.DefaultChallengeScheme = "Smart";
    })
    .AddPolicyScheme("Smart", "Cookie or API token", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? ApiTokenAuthenticationHandler.SchemeName
                : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "OfficeDays.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = cookieExpireTimeSpan;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(ApiTokenAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireClaim("is_admin", "true"));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing")) app.UseHsts();
app.UseBearerTokenHttpsProtection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync(app.Lifetime.ApplicationStopping);

app.MapApiEndpoints();
OfficeDays.Features.Operations.OperationsEndpoints.Map(app);
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
