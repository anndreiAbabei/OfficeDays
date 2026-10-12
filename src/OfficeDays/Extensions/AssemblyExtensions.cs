using System.Reflection;

namespace OfficeDays.Extensions;

public static class AssemblyExtensions
{
    public static string GetMetadata(this Assembly assembly, string key, string defaultValue)
    {
        return assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                       .FirstOrDefault(x => x.Key == key)?.Value ?? defaultValue;
    }
}