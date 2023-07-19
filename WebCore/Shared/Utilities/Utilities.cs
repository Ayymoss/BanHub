using System.Net;
using System.Reflection;

namespace BanHub.WebCore.Shared.Utilities;

public static class Utilities
{
    public static string GetVersionAsString()
    {
        return Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    public static bool IsInternal(this string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var address)) return false;
        if (ipAddress.StartsWith("127.0.0")) return true;
        var bytes = address.GetAddressBytes();

        return bytes[0] switch
        {
            0 => bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0,
            10 => true,
            172 => bytes[1] < 32 && bytes[1] >= 16,
            192 => bytes[1] == 168,
            _ => false
        };
    }
}
