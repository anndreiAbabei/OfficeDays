namespace OfficeDays.Domain;

public sealed class Vacation
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    
    public User User { get; init; } = null!;
}
