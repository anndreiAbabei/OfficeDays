namespace OfficeDays.Domain;

public sealed class User
{
    public Guid Id { get; init; }
    public required string Username { get; init; }
    public required string NormalizedUsername { get; init; }
    public required string PasswordHash { get; set; }
    public required string TimeZoneId { get; init; }
    public bool IsAdmin { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<Attendance> Attendances { get; init; } = [];
    public List<Vacation> Vacations { get; init; } = [];
    public List<ApiToken> ApiTokens { get; init; } = [];
}

public sealed class Attendance
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public User User { get; set; } = null!;
    public DateOnly Date { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class Vacation
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public User User { get; set; } = null!;
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class ApiToken
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public User User { get; set; } = null!;
    public required string Name { get; init; }
    public required string TokenHash { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}

public sealed class BankHoliday
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public required string Name { get; init; }
}
