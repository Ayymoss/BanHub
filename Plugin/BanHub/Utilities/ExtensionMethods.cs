namespace BanHub.Utilities;

public static class ExtensionMethods
{
    public static string? GetDomainName(this string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) && !Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) return null;
        return uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
    }
}
