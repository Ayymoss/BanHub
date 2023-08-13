using System.Text.RegularExpressions;

namespace BanHub.WebCore.Server.Utilities;

public static partial class ExtensionMethods
{
    public static string FilterUnknownCharacters(this string input)
    {
        var cleanedName = MyRegex().Replace(input, "?");
        return string.IsNullOrEmpty(cleanedName) ? "Unknown" : cleanedName;
    }

    public static string? GetDomainName(this string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) && !Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) return null;
        return uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
    }

    [GeneratedRegex(@"[^\p{L}\p{P}\p{N}]")]
    private static partial Regex MyRegex();
}
