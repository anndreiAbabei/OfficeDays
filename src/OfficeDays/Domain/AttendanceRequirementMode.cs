using System.Text.Json.Serialization;

namespace OfficeDays.Domain;

[JsonConverter(typeof(JsonStringEnumConverter<AttendanceRequirementMode>))]
public enum AttendanceRequirementMode
{
    Percentage,
    DaysPerWeek
}
