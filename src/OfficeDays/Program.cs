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

AddHealthCheck(builder);
AddKeyPath(builder);

AddAntiforgery(builder);

AddDatabase(builder);
AddServices(builder);

AddAuthorization(builder, cookieExpireTimeSpan);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing")) 
    app.UseHsts();

app.UseBearerTokenHttpsProtection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync(app.Lifetime.ApplicationStopping);

app.MapApiEndpoints();

OfficeDays.Features.Operations.OperationsEndpoints.Map(app);

app.MapFallbackToFile("index.html");

await app.RunAsync();
return;

void AddHealthCheck(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddProblemDetails();
    webApplicationBuilder.Services.AddHealthChecks()
                         .AddCheck<OfficeDays.Features.Operations.DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(5));
}

void AddKeyPath(WebApplicationBuilder kpBuilder)
{
    var keysPath = kpBuilder.Configuration["DataProtection:KeysPath"];
    if (string.IsNullOrWhiteSpace(keysPath))
        kpBuilder.Services.AddDataProtection();
    else
    {
        Directory.CreateDirectory(keysPath);
        kpBuilder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));
    }
}

void AddAntiforgery(WebApplicationBuilder antiForgeBuilder)
{
    antiForgeBuilder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.Name = "OfficeDays.Antiforgery";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
}

void AddDatabase(WebApplicationBuilder dbBuilder)
{
    dbBuilder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(dbBuilder.Configuration.GetConnectionString("Default") ?? 
                                                                              "Data Source=officedays.db"));
    dbBuilder.Services.AddSingleton<DatabaseInitializer>();
}

void AddServices(WebApplicationBuilder svcBuilder)
{
    svcBuilder.Services.AddSingleton(TimeProvider.System);
    svcBuilder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
    svcBuilder.Services.AddScoped<UserDateService>();
    svcBuilder.Services.AddScoped<CookieAntiforgeryFilter>();
}

void AddAuthorization(WebApplicationBuilder authBuilder, TimeSpan timeSpan)
{
    authBuilder.Services.AddAuthentication(options =>
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
                   options.ExpireTimeSpan = timeSpan;
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
               .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(ApiTokenAuthenticationHandler.SchemeName, _ =>
               {
               });

    authBuilder.Services.AddAuthorizationBuilder()
               .AddPolicy("AdminOnly", policy => policy.RequireClaim("is_admin", "true"));
}

public partial class Program;
