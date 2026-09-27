using Microsoft.AspNetCore.HttpOverrides;
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

app.MapApiEndpoints();

await app.Services.GetRequiredService<DatabaseInitializer>()
                  .InitializeAsync(app.Lifetime.ApplicationStopping);

await app.RunAsync();
return;

namespace OfficeDays
{
    public partial class Program;
}
