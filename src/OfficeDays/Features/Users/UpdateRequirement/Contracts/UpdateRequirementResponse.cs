namespace OfficeDays.Features.Users.UpdateRequirement.Contracts;

public sealed record UpdateRequirementResponse(Guid Id,
                                               DateOnly StartingFrom,
                                               int RequiredOfficePercentage,
                                               DateTimeOffset CreatedAt,
                                               bool IsBaseline);
