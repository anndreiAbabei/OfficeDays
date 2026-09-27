namespace OfficeDays.Features.Users.CreateUser;

public static partial class CreateUserHandlerLogging
{
    private const int EventIdStart = 2200;

    [LoggerMessage(EventIdStart + 1, LogLevel.Warning, "A duplicate username registration was attempted for {Username}")]
    public static partial void LogDuplicateUsername(this ILogger<CreateUserHandler> logger, string? username);

    [LoggerMessage(EventIdStart + 2, LogLevel.Information, "Created normal user {Username} with user ID {UserId}")]
    public static partial void LogUserCreated(this ILogger<CreateUserHandler> logger, string? username, Guid userId);

    [LoggerMessage(EventIdStart + 3, LogLevel.Warning, "Holiday jurisdiction {JurisdictionCode} was not found")]
    public static partial void LogJurisdictionNotFound(this ILogger<CreateUserHandler> logger, string jurisdictionCode);
}
