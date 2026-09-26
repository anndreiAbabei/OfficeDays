using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Endpoints;
using OfficeDays.Extensions;
using OfficeDays.Infrastructure;
using OfficeDays.Middleware;
using OfficeDays.Security;
using OfficeDays.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
var cookieExpireTimeSpan = builder.Configuration.GetValue<TimeSpan>("Authentication:CookieExpireTimeSpan");
if (cookieExpireTimeSpan <= TimeSpan.Zero)
    throw new InvalidOperationException("Authentication:CookieExpireTimeSpan must be a positive TimeSpan.");

AddHealthCheck(builder);
AddKeyPath(builder);

AddAntiforgery(builder);

AddDatabase(builder);
AddServices(builder);

AddEndpoints(builder);
AddHandlers(builder);
AddValidation(builder);

AddAuthorization(builder, cookieExpireTimeSpan);

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    app.UseHsts();

app.UseBearerTokenHttpsProtection();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync(app.Lifetime.ApplicationStopping);

app.MapApiEndpoints();

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
    svcBuilder.Services.AddScoped<CookieAntiforgeryFilter>();
    svcBuilder.Services.AddSingleton<IUserDateService, UserDateService>();
    svcBuilder.Services.AddSingleton<IRequestExecutor, RequestExecutor>();
    svcBuilder.Services.AddHttpContextAccessor();
    svcBuilder.Services.AddSingleton<OfficeDays.Features.Ui.UiContent>();
    svcBuilder.Services.AddScoped<ICurrentUser, CurrentUser>();
}

void AddEndpoints(WebApplicationBuilder endpointsBuilder)
{
    var assembly = typeof(OfficeDays.Program).Assembly;
    var groupInterface = typeof(IEndpointGroup);
    var groupTypes = assembly.GetTypesImplementing<IEndpointGroup>();

    foreach (var type in groupTypes)
        endpointsBuilder.Services.AddSingleton(groupInterface, type);

    var endpointInterfaceTypes = assembly.GetTypesImplementing<IEndpoint>(false)
                                         .Where(t => t.IsInterface);

    foreach (var endpointInterfaceType in endpointInterfaceTypes)
    {
        var endpointTypes = assembly.GetTypesImplementing(endpointInterfaceType);

        foreach (var type in endpointTypes)
            endpointsBuilder.Services.AddSingleton(endpointInterfaceType, type);
    }
}

void AddHandlers(WebApplicationBuilder handlersBuilder)
{
    var handlerInterface = typeof(IRequestHandler<>);
    var types = typeof(OfficeDays.Program).Assembly.GetTypesImplementing(handlerInterface);

    foreach (var type in types)
    {
        var requestType = type.GetInterfaces()
                              .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                              .Select(i => i.GetGenericArguments()[0])
                              .First();
        handlersBuilder.Services.AddScoped(handlerInterface.MakeGenericType(requestType), type);
    }
}

void AddValidation(WebApplicationBuilder validationBuilder)
{
    validationBuilder.Services.AddValidatorsFromAssembly(typeof(OfficeDays.Program).Assembly);
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

namespace OfficeDays
{
    public partial class Program;
}
