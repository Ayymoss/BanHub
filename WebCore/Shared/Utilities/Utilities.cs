using System.Reflection;

namespace BanHub.WebCore.Shared.Utilities;

public static class Utilities
{
    public static string GetVersionAsString()
    {
        return Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }
}
