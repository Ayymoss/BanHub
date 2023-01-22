using System.Net.Http.Json;
using GlobalInfraction.WebCore.Client.Interfaces;
using GlobalInfraction.WebCore.Shared.DTOs.WebEntity;

namespace GlobalInfraction.WebCore.Client.Services;

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
        Console.WriteLine(response.StatusCode + " " + response.IsSuccessStatusCode);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }

    public async Task<(string message, UserDto? user)> UserProfileAsync()
    {
        var response = await _httpClient.GetAsync("/api/v2/Auth/Profile");
        if (response.IsSuccessStatusCode)
        {
            return ("Success", await response.Content.ReadFromJsonAsync<UserDto>());
        }

        return response.StatusCode is System.Net.HttpStatusCode.Unauthorized ? ("Unauthorized", null) : ("Failed", null);
    }

    public async Task<string> LogoutAsync()
    {
        var response = await _httpClient.PostAsync("/api/v2/Auth/Logout", null);
        return response.IsSuccessStatusCode ? "Success" : "Failed";
    }
}
