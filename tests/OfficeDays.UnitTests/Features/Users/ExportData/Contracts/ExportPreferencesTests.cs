using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportPreferencesTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedTeamDay = "thursday";

        // act
        var sut = new ExportPreferences(expectedTeamDay);

        // assert
        Assert.Equal(expectedTeamDay, sut.TeamDay);
    }
}
