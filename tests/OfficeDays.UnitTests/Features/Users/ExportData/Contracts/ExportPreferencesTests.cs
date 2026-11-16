using OfficeDays.Domain;
using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportPreferencesTests
{
    [Fact]
    public void Should_SetProperties_WhenPreferencesAreProvided()
    {
        // arrange
        const Appearance expectedAppearance = Appearance.Light;
        const string expectedTeamDay = "thursday";

        // act
        var sut = new ExportPreferences(expectedTeamDay, expectedAppearance);

        // assert
        Assert.Equal(expectedAppearance, sut.Appearance);
        Assert.Equal(expectedTeamDay, sut.TeamDay);
    }
    [Fact]
    public void Should_UseSystem_WhenAppearanceIsOmitted()
    {
        // Arrange
        const Appearance expectedAppearance = Appearance.System;

        // Act
        var preferences = new ExportPreferences(null);

        // Assert
        Assert.Equal(expectedAppearance, preferences.Appearance);
    }
}
