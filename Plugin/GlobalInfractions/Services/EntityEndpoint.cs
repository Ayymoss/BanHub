using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class EntityEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://globalinfractions.com/api/v2";
#endif

    public EntityEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<EntityDto?> GetEntity(string identity)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiHost}/Entity?identity={identity}");

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<EntityDto>();
            return json;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending instance heartbeat: {e.Message}");
        }

        return null;
    }

    public async Task<bool> UpdateEntity(EntityDto entity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiHost}/Entity?authToken={_configurationModel.ApiKey}", entity);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }

    public async Task<bool> HasEntity(string identity)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiHost}/Entity/Exists?identity={identity}");
            if (!response.IsSuccessStatusCode) return false;
            var content = await response.Content.ReadAsStringAsync();
            var boolParse = bool.TryParse(content, out var result);
            return boolParse && result;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }
}
