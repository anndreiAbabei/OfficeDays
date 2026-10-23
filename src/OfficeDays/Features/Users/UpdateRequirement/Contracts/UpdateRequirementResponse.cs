using OfficeDays.Domain;

namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementResponse(Guid Id,
                                               DateOnly StartingFrom,
                                               int RequirementValue,
                                               DateTimeOffset CreatedAt,
                                               bool IsBaseline)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
