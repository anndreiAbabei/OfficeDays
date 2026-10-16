namespace OfficeDays.Features.Users.GetRequirements.Contracts;

public sealed record GetRequirementsItem(Guid Id,
                                         DateOnly StartingFrom,
                                         int RequiredOfficePercentage,
                                         DateTimeOffset CreatedAt,
                                         bool IsBaseline);
