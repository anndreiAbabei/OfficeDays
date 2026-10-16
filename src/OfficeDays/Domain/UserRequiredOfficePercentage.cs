namespace OfficeDays.Domain;

public sealed class UserRequiredOfficePercentage
{
    // The baseline applies to all history, including dates before registration.
    public static readonly DateOnly BaselineDate = DateOnly.MinValue;
    
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateOnly StartingFrom { get; set; }
    public int RequiredOfficePercentage { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
