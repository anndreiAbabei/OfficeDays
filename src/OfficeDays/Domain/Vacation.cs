namespace OfficeDays.Domain;

public sealed class Vacation
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    public User User { get; init; } = null!;
}
