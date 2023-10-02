using System.Net;

namespace BanHub.Application.Utilities;

public static class Generic
{
    public static bool IsDebug { get; set; }
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
