using OfficeDays.Features.Users.ExportData.Contracts;

namespace OfficeDays.UnitTests.Features.Users.ExportData.Contracts;

public sealed class ExportApplicationTests
{
    [Fact]
    public void Should_SetProperties()
    {
        // arrange
        const string expectedName = "OfficeDays";
        const string expectedVersion = "2.2.1+test";

        // act
        var sut = new ExportApplication(expectedName, expectedVersion);

        // assert
        Assert.Equal(expectedName, sut.Name);
        Assert.Equal(expectedVersion, sut.Version);
    }
}
