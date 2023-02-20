using System.Security.Principal;
using System.Web;

namespace BanHub.WebCore.Shared.Utilities;

public static class ExtensionMethods
{
    public static bool IsInAnyRole(this IPrincipal principal, params string[] roles) => roles.Any(principal.IsInRole);

    public static Uri ParseUri(this HttpClient http, string endPoint, object? query)
    {
        var uriBuilder = new UriBuilder
        {
            Path = endPoint,
            Host = http.BaseAddress!.Host,
            Port = http.BaseAddress.Port,
            Query = string.Join("&", query?
                                         .GetType()
                                         .GetProperties()
                                         .Select(prop =>
                                             $"{prop.Name.ToLower()}={Uri.EscapeDataString(prop.GetValue(query)?.ToString() ?? string.Empty)}")
                                     ?? Array.Empty<string>())
        };

        return uriBuilder.Uri;
    }
}
