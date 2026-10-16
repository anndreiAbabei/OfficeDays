namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public required int RequiredOfficePercentage { get; init; }
}
