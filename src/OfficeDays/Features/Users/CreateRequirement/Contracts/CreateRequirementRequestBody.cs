using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public AttendanceRequirementMode? RequirementMode { get; init; }
    public int? RequirementValue { get; init; }
}
