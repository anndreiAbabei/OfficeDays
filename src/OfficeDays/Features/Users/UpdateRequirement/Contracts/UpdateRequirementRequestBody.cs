using OfficeDays.Domain;

namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public AttendanceRequirementMode? Mode { get; init; }
    public int? Value { get; init; }
    public int? RequiredOfficePercentage { get; init; }
}
