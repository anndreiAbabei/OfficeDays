namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementRequestBody
{
    public required DateOnly StartingFrom { get; init; }
    public required int RequiredOfficePercentage { get; init; }
}
