using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementResponse(Guid Id,
                                               DateOnly StartingFrom,
                                               int RequirementValue,
                                               DateTimeOffset CreatedAt,
                                               bool IsBaseline)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
