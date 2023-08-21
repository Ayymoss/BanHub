using System.Security.Principal;
using System.Text.Json;
using BanHubData.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace BanHub.WebCore.Client.Utilities;

public static class ExtensionMethods
{
    public static bool IsInEqualOrHigherRole<TEnum>(this IPrincipal principal, TEnum role) where TEnum : Enum
    {
        var roleValue = Convert.ToInt32(role);

        var higherRoles = Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Where(r => Convert.ToInt32(r) >= roleValue);

        return higherRoles.Any(higherRole => principal.IsInRole(higherRole.ToString()));
    }

    public static string GetGameName(this Game game)
    {
        return game switch
        {
            Game.COD => "Call of Duty",
            Game.IW3 => "Modern Warfare",
            Game.IW4 => "Modern Warfare 2",
            Game.IW5 => "Modern Warfare 3",
            Game.IW6 => "Modern Warfare 2",
            Game.T4 => "World at War",
            Game.T5 => "Black Ops",
            Game.T6 => "Black Ops 2",
            Game.T7 => "Black Ops 3",
            Game.SHG1 => "Advanced Warfare",
            Game.CSGO => "Counter-Strike: Global Offensive",
            Game.H1 => "Modern Warfare Remastered",
            _ => "Unknown"
        };
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


    public static bool TryGetQueryString<T>(this NavigationManager navManager, string key, out T value)
    {
        var uri = navManager.ToAbsoluteUri(navManager.Uri);

        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
        {
            if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
            {
                value = (T)(object)valueAsInt;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                value = (T)(object)valueFromQueryString.ToString();
                return true;
            }

            if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
            {
                value = (T)(object)valueAsDecimal;
                return true;
            }
        }

        value = default;
        return false;
    }
}
