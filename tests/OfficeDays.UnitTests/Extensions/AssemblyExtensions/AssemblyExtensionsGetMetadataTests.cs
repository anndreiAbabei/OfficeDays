using System.Reflection;
using OfficeDays.Extensions;

[assembly: AssemblyMetadata(OfficeDays.UnitTests.Extensions.AssemblyExtensionsGetMetadataTests.ExpectedAssemblyMetadataKey, 
                            OfficeDays.UnitTests.Extensions.AssemblyExtensionsGetMetadataTests.ExpectedAssemblyMetadataValue)]

namespace OfficeDays.UnitTests.Extensions;

public sealed class AssemblyExtensionsGetMetadataTests
{
    internal const string ExpectedAssemblyMetadataKey = $"{nameof(AssemblyExtensionsGetMetadataTests)}-test-assembly-metadata-key";
    internal const string ExpectedAssemblyMetadataValue = $"{nameof(AssemblyExtensionsGetMetadataTests)}-test-assembly-metadata-value";
    
    [Fact]
    public void Should_ReturnMetadataFromAssembly()
    {
        // arrange
        var assembly = typeof(AssemblyExtensionsGetMetadataTests).Assembly;

        // act
        var result = assembly.GetMetadata(ExpectedAssemblyMetadataKey, "");

        // assert
        Assert.Equal(ExpectedAssemblyMetadataValue, result);
    }

    [Fact]
    public void Should_ReturnDefaultValue_WhenDoesNotExists()
    {
        // arrange
        var assembly = typeof(AssemblyExtensionsGetMetadataTests).Assembly;
        const string expectedDefaultValue = "no-value";
        
        // act
        var result = assembly.GetMetadata($"{nameof(AssemblyExtensionsGetMetadataTests)}-{Guid.NewGuid()}", expectedDefaultValue);

        // assert
        Assert.Equal(expectedDefaultValue, result);
    }
}
