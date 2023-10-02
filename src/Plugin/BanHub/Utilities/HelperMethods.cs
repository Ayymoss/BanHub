using System.Text.RegularExpressions;
using System.Web;

namespace BanHub.Plugin.Utilities;

public static class HelperMethods
{
    public static string ObscureGuid(string input)
    {
        var guidPattern = @"(\b[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}\b)";

        return Regex.Replace(input, guidPattern, m =>
            $"{m.Value[..8]}-****-****-****-{m.Value[24..]}");
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
