using OfficeDays.Domain;
using OfficeDays.Features.Users.GetRequirements.Contracts;
using OfficeDays.Services;

namespace OfficeDays.Features.Users.GetRequirements;

public static class GetRequirementsMapping
{
    public static GetRequirementsResponse ToViewModel(this User user, IEnumerable<UserAttendanceRequirement> requirements, IUserDateService dates)
    {
        return new GetRequirementsResponse(dates.Today(user), 
                                           requirements.Select(row => row.ToViewModel()));
    }

    public static GetRequirementsItem ToViewModel(this UserAttendanceRequirement row)
    {
        return new GetRequirementsItem(row.Id,
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
