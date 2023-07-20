using System.Web;

namespace BanHubData.Extension;

public static class Extensions
{
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
