using System.Diagnostics.CodeAnalysis;
using OfficeDays.Security.RateLimiting;
using OfficeDays.Configuration;
using OfficeDays.Data;
using OfficeDays.Endpoints;
using OfficeDays.Extensions;
using OfficeDays.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();
builder.Services.AddFeatures();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseForwardedHeaders();
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    app.UseHsts();

// Compress only public build assets, never cookie-authenticated API responses.
app.UseWhen(context => context.Request.Path.StartsWithSegments("/_app"),
    assets => assets.UseResponseCompression());
app.UseBearerTokenHttpsProtection();
app.UseRouting();
app.UseMiddleware<RequestLimitingMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapApiEndpoints();

await app.Services.GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync(app.Lifetime.ApplicationStopping);

await app.RunAsync();

// ReSharper disable once RedundantJumpStatement -- it's and end marker
return;

namespace OfficeDays
{
    [SuppressMessage("ReSharper", "PartialTypeWithSinglePart", Justification = "Used for integration testing")]
    public partial class Program;
}
