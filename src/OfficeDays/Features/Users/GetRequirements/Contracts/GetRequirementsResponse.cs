namespace OfficeDays.Features.Users.GetRequirements.Contracts;

public sealed record GetRequirementsResponse(DateOnly Today, IEnumerable<GetRequirementsItem> Items);
