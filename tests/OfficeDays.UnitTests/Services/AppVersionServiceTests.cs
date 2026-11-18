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

    [Fact]
    public void Should_ReturnBuildMetadata_WhenReadingAllVersionInfo()
    {
        // Arrange
        var service = new AppVersionService();
        var assembly = typeof(Program).Assembly;
        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(x => x.Key, x => x.Value);
        var expected = new AppVersionInfo(service.Version, metadata["Authors"]!, metadata["RepositoryUrl"]!);

        // Act
        var info = service.GetAll();

        // Assert
        Assert.Equal(expected, info);
    }
}
