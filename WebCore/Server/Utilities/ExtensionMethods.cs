using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Radzen;

namespace BanHub.WebCore.Server.Utilities;

public static class ExtensionMethods
{
    public static string FilterUnknownCharacters(this string input)
    {
        // Ignoring request since it seems changed in .NET 8
        var regex = new Regex(@"[^\p{L}\p{P}\p{N}]");
        var cleanedName = regex.Replace(input, "?");
        return string.IsNullOrEmpty(cleanedName) ? "Unknown" : cleanedName;
    }

    public static IQueryable<TDomain> ApplySort<TDomain>(this IQueryable<TDomain> query, SortDescriptor sort,
        Expression<Func<TDomain, object>> property)
    {
        return sort.SortOrder is SortOrder.Ascending
            ? query.OrderBy(property)
            : query.OrderByDescending(property);
    }

    public static string? GetDomainName(this string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) && !Uri.TryCreate("http://" + url, UriKind.Absolute, out uri)) return null;
        return uri.Host.StartsWith("www.") ? uri.Host[4..] : uri.Host;
    }
}
