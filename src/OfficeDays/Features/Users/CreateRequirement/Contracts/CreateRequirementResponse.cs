namespace OfficeDays.Features.Users.CreateRequirement.Contracts;

public sealed record CreateRequirementResponse(Guid Id,
                                               DateOnly StartingFrom,
                                               int RequiredOfficePercentage,
                                               DateTimeOffset CreatedAt,
                                               bool IsBaseline);
