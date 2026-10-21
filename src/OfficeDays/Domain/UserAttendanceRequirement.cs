namespace OfficeDays.Domain;

public sealed class UserAttendanceRequirement
{
    // The baseline applies to all history, including dates before registration.
    public static readonly DateOnly BaselineDate = DateOnly.MinValue;
    
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateOnly StartingFrom { get; set; }
    public AttendanceRequirementMode Mode { get; set; }
    public int Value { get; set; }

    public int RequiredOfficePercentage => Mode switch
    {
        AttendanceRequirementMode.Percentage => Value,
        AttendanceRequirementMode.DaysPerWeek => Value * 20,
        _ => throw new InvalidOperationException("Unknown attendance requirement mode.")
    };
    public DateTimeOffset CreatedAt { get; init; }
}
