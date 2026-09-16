namespace OfficeDays.Domain;

public sealed class Attendance
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public User User { get; set; } = null!;
    public DateOnly Date { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
