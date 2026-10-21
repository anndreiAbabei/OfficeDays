using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public AttendanceRequirementMode? Mode { get; init; }
    public int? Value { get; init; }
    public int? RequiredOfficePercentage { get; init; }
}
