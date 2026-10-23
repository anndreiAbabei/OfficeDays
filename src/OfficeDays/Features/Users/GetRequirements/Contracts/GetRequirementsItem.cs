using OfficeDays.Domain;

namespace OfficeDays.Features.Users.GetRequirements.Contracts;

public sealed record GetRequirementsItem(Guid Id,
                                         DateOnly StartingFrom,
                                         int RequirementValue,
                                         DateTimeOffset CreatedAt,
                                         bool IsBaseline)
{
    public AttendanceRequirementMode RequirementMode { get; init; } = AttendanceRequirementMode.Percentage;
}
