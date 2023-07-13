using System.Net.Http.Json;
using System.Text.Json;
using BanHub.WebCore.Client.Interfaces;
using BanHub.WebCore.Shared.Commands.Web;
using BanHub.WebCore.Shared.Models.Shared;

namespace BanHub.WebCore.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> LoginAsync(WebTokenLoginCommand webLogin)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/Auth/Login", webLogin);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }

    public async Task<(string, WebUser?)> UserProfileAsync()
    {
        var response = await _httpClient.GetAsync("/api/Auth/Profile");
        if (response.IsSuccessStatusCode)
        {
            WebUser? result;
            try
            {
                result = await response.Content.ReadFromJsonAsync<WebUser>();
            }
            catch (JsonException)
            {
                return ("Failed", null);
            }

            return ("Success", result);
        }

        return response.StatusCode is System.Net.HttpStatusCode.Unauthorized ? ("Unauthorized", null) : ("Failed", null);
    }

    public async Task<string> LogoutAsync()
    {
        var response = await _httpClient.PostAsync("/api/Auth/Logout", null);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }
}
