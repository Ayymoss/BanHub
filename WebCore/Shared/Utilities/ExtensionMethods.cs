using System.Security.Principal;
using System.Text.Json;
using System.Web;

namespace BanHub.WebCore.Shared.Utilities;

public static class ExtensionMethods
{
    public static bool IsInAnyRole(this IPrincipal principal, params string[] roles) => roles.Any(principal.IsInRole);

    public static Uri ParseUri(this HttpClient http, string endPoint, object? query)
    {
        var uriBuilder = new UriBuilder
        {
#if DEBUG
            Scheme = "http",
#else
            Scheme = "https",
#endif
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

    public static async Task<TResponse?> DeserializeHttpResponseContentAsync<TResponse>(this HttpResponseMessage response)
        where TResponse : class
    {
        try
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<TResponse>(json, jsonSerializerOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
}
