namespace OfficeDays.Domain;

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
