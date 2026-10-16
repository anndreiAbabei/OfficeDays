using OfficeDays.Domain;

namespace OfficeDays.Features.Users.Requirements;

public sealed record RequirementResponse(Guid Id, 
                                         DateOnly StartingFrom,
                                         int RequiredOfficePercentage,
                                         DateTimeOffset CreatedAt, 
                                         bool IsBaseline)
{
    public static RequirementResponse From(UserRequiredOfficePercentage row) => new(row.Id, row.StartingFrom,
        row.RequiredOfficePercentage, row.CreatedAt, row.StartingFrom == UserRequiredOfficePercentage.BaselineDate);
}
