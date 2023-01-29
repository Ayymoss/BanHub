using System.Net.Http.Json;
using System.Text.Json;
using BanHub.WebCore.Client.Interfaces;
using BanHub.WebCore.Shared.DTOs.WebEntity;

namespace BanHub.WebCore.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> LoginAsync(LoginRequestDto login)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v2/Auth/Login", login);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }

    public async Task<(string, UserDto?)> UserProfileAsync()
    {
        var response = await _httpClient.GetAsync("/api/v2/Auth/Profile");
        if (response.IsSuccessStatusCode)
        {
            UserDto? result;
            try
            {
                result = await response.Content.ReadFromJsonAsync<UserDto>();
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
        var response = await _httpClient.PostAsync("/api/v2/Auth/Logout", null);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }
}
