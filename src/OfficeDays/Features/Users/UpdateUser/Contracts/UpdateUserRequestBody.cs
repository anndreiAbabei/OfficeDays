using System.Text.Json.Serialization;

namespace OfficeDays.Features.Users.UpdateUser.Contracts;

public sealed record UpdateUserRequestBody(string? Email)
{
    public string? Appearance { get; init; }

    public string? TeamDay
    {
        get;
        init
        {
            field = value;
            HasTeamDay = true;
        }
    }

    [JsonIgnore]
    public bool HasTeamDay { get; private init; }
}
