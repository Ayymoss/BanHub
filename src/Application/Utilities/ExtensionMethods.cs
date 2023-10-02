using System.Text.RegularExpressions;
using System.Web;

namespace BanHub.Application.Utilities;

public static class ExtensionMethods
{
    public static string FilterUnknownCharacters(this string input)
    {
        // Ignoring request since it seems changed in .NET 8
        var regex = new Regex(@"[^\p{L}\p{P}\p{N}]");
        var cleanedName = regex.Replace(input, "?");
        return string.IsNullOrEmpty(cleanedName) ? "Unknown" : cleanedName;
    }

    public static string? GetDomainName(this string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) && !Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) return null;
        return uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
    }

    public static string? GetYouTubeVideoId(this string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;

        var host = uri.Host;
        if (host.Contains("youtube.com"))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (query.AllKeys.Contains("v")) return query["v"];
            if (uri.AbsolutePath.Contains("shorts")) return uri.AbsolutePath.Replace("/shorts/", "");
        }

        return host.Contains("youtu.be") ? uri.AbsolutePath.Replace("/", "") : null;
    }
}
