using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Domain;
using OfficeDays.Services;

namespace OfficeDays.Data;

public sealed class DatabaseInitializer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        TimeProvider timeProvider,
        ILogger<DatabaseInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        _logger.LogInformation("Database migrations are up to date");

        if (await db.Users.AnyAsync())
        {
            _logger.LogDebug("Skipping bootstrap administrator creation because users already exist");
            return;
        }

        var password = _configuration["BootstrapAdmin:Password"];
        var timeZoneId = _configuration["BootstrapAdmin:TimeZoneId"];
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(timeZoneId))
        {
            _logger.LogCritical("Bootstrap administrator environment variables are missing for an empty database");
            throw new InvalidOperationException("The database has no users. Set the BootstrapAdmin__Password and BootstrapAdmin__TimeZoneId environment variables to create the initial Admin account.");
        }
        if (password.Length < 12)
            throw new InvalidOperationException("The BootstrapAdmin__Password environment variable must contain at least 12 characters.");
        if (!UserDateService.IsValidTimeZone(timeZoneId))
            throw new InvalidOperationException($"BootstrapAdmin__TimeZoneId '{timeZoneId}' is not a valid IANA timezone.");

        var user = new User
        {
            Id = Guid.NewGuid(), Username = "Admin", NormalizedUsername = "ADMIN", PasswordHash = string.Empty,
            TimeZoneId = timeZoneId, IsAdmin = true,
            CreatedAt = _timeProvider.GetUtcNow()
        };
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        _logger.LogInformation("Created bootstrap administrator {Username} with user ID {UserId}", user.Username, user.Id);
    }
}
