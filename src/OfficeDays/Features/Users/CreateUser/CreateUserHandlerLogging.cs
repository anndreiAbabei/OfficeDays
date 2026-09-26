namespace OfficeDays.Features.Users.CreateUser;

public static partial class CreateUserHandlerLogging
{
    [LoggerMessage(2201, LogLevel.Warning, "A duplicate username registration was attempted for {Username}")]
    public static partial void LogDuplicateUsername(this ILogger<CreateUserHandler> logger, string? username);

    [LoggerMessage(2202, LogLevel.Information, "Created normal user {Username} with user ID {UserId}")]
    public static partial void LogUserCreated(this ILogger<CreateUserHandler> logger, string? username, Guid userId);
}
