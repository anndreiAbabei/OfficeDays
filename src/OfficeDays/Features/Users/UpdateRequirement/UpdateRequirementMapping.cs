using OfficeDays.Domain;
using OfficeDays.Features.Users.UpdateRequirement.Contracts;

namespace OfficeDays.Features.Users.UpdateRequirement;

public static class UpdateRequirementMapping
{
    public static UpdateRequirementResponse ToViewModel(this UserAttendanceRequirement row)
    {
        return new UpdateRequirementResponse(row.Id,
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
