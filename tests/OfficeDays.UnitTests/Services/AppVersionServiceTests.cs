using System.Reflection;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Services;

public sealed class AppVersionServiceTests
{
    [Fact]
    public void Should_ReturnApplicationInformationalVersionIncludingBuildMetadata()
    {
        // arrange
        var expectedVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        var sut = new AppVersionService();

        // act
        var version = sut.Version;

        // assert
        Assert.Equal(expectedVersion, version);
    }
}
