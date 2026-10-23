using OfficeDays.Domain;

namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public AttendanceRequirementMode? RequirementMode { get; init; }
    public int? RequirementValue { get; init; }
}
