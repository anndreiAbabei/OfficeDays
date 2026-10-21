using OfficeDays.Domain;

namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementResponse(Guid Id,
                                               DateOnly StartingFrom,
                                               int RequiredOfficePercentage,
                                               DateTimeOffset CreatedAt,
                                               bool IsBaseline)
{
    public AttendanceRequirementMode Mode { get; init; } = AttendanceRequirementMode.Percentage;
    public int Value { get; init; } = RequiredOfficePercentage;
}
