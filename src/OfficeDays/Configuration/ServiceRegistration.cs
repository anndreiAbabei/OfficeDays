using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.Configuration;

public static class ServiceRegistration
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });
        var cookieExpireTimeSpan = builder.Configuration.GetValue<TimeSpan>("Authentication:CookieExpireTimeSpan");
        if (cookieExpireTimeSpan <= TimeSpan.Zero)
            throw new InvalidOperationException("Authentication:CookieExpireTimeSpan must be a positive TimeSpan.");

        builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
        AddHealthCheck(builder);
        AddKeyPath(builder);
        AddAntiforgery(builder);
        AddDatabase(builder);
        AddServices(builder);
        AddAuthorization(builder, cookieExpireTimeSpan);
        return builder;
    }

    private static void AddHealthCheck(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.AddProblemDetails();
        webApplicationBuilder.Services.AddHealthChecks()
                             .AddCheck<OfficeDays.Features.Operations.DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(5));
    }

    private static void AddKeyPath(WebApplicationBuilder kpBuilder)
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

    private static void AddAntiforgery(WebApplicationBuilder antiForgeBuilder)
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

    private static void AddDatabase(WebApplicationBuilder dbBuilder)
    {
        dbBuilder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(dbBuilder.Configuration.GetConnectionString("Default") ??
                                                                                  "Data Source=officedays.db"));
        dbBuilder.Services.AddSingleton<DatabaseInitializer>();
    }

    private static void AddServices(WebApplicationBuilder svcBuilder)
    {
        svcBuilder.Services.AddSingleton(TimeProvider.System);
        svcBuilder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        svcBuilder.Services.AddScoped<CookieAntiforgeryFilter>();
        svcBuilder.Services.AddSingleton<IUserDateService, UserDateService>();
        svcBuilder.Services.AddSingleton<IRequestExecutor, RequestExecutor>();
        svcBuilder.Services.AddSingleton<IUserService, UserService>();
        svcBuilder.Services.AddSingleton<IAttendanceCalculator, AttendanceCalculator>();
        svcBuilder.Services.AddSingleton<IApiTokenService, ApiTokenService>();
        svcBuilder.Services.AddHttpContextAccessor();
        svcBuilder.Services.AddSingleton<OfficeDays.Features.Ui.UiContent>();
        svcBuilder.Services.AddScoped<ICurrentUser, CurrentUser>();
    }

    private static void AddAuthorization(WebApplicationBuilder authBuilder, TimeSpan timeSpan)
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
}
