using System.Text.Json.Serialization;

namespace OfficeDays.Domain;

[JsonConverter(typeof(JsonStringEnumConverter<AttendanceRequirementMode>))]
public enum AttendanceRequirementMode
{
    Percentage,
    DaysPerWeek
}

public sealed class UserAttendanceRequirement
{
    // The baseline applies to all history, including dates before registration.
    public static readonly DateOnly BaselineDate = DateOnly.MinValue;
    
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateOnly StartingFrom { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public AttendanceRequirementMode RequirementMode { get; set; }
    public int RequirementValue { get; set; }
    
    public int EffectivePercentage => RequirementMode switch
    {
        AttendanceRequirementMode.Percentage => RequirementValue,
        AttendanceRequirementMode.DaysPerWeek => RequirementValue * 20,
        _ => throw new InvalidOperationException("Unknown attendance requirement mode.")
    };
}
