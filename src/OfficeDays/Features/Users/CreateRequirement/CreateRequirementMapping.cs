using OfficeDays.Domain;
using OfficeDays.Features.Users.CreateRequirement.Contracts;

namespace OfficeDays.Features.Users.CreateRequirement;

public static class CreateRequirementMapping
{
    public static CreateRequirementResponse ToViewModel(this UserAttendanceRequirement row)
    {
        return new CreateRequirementResponse(row.Id,
                                             row.StartingFrom,
                                             row.RequiredOfficePercentage,
                                             row.CreatedAt,
                                             row.StartingFrom == UserAttendanceRequirement.BaselineDate)
        {
            Mode = row.Mode,
            Value = row.Value
        };
    }
}
