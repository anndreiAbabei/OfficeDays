using System.Text.Json;
using OfficeDays.Domain;
using OfficeDays.Features.Users.UpdateUser;
using OfficeDays.Features.Users.UpdateUser.Contracts;

namespace OfficeDays.UnitTests.Domain;

public sealed class TeamDayTests
{
    [Theory]
    [InlineData("{}", false, null)]
    [InlineData("{\"teamDay\":null}", true, null)]
    [InlineData("{\"teamDay\":\"thursday\"}", true, "thursday")]
    public void Deserialization_distinguishes_omission_from_clearing(string json, bool present, string? day)
    {
        var body = JsonSerializer.Deserialize<UpdateUserRequestBody>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(present, body.HasTeamDay);
        Assert.Equal(day, body.TeamDay);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("monday", true)]
    [InlineData("tuesday", true)]
    [InlineData("wednesday", true)]
    [InlineData("thursday", true)]
    [InlineData("friday", true)]
    [InlineData("saturday", false)]
    [InlineData("sunday", false)]
    [InlineData("", false)]
    [InlineData("unknown", false)]
    [InlineData("4", false)]
    public void Validates_workdays_only(string? day, bool valid)
    {
        var result = new UpdateUserRequestBodyValidator().Validate(new UpdateUserRequestBody(null) { TeamDay = day });
        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public void Preferences_default_to_no_team_day()
    {
        Assert.Null(new UserPreferences().TeamDay);
    }
}
