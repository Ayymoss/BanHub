using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;

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
            return await response.Content.ReadFromJsonAsync<EntityDto>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error getting entity: {e.Message}");
        }

        return null;
    }

    public async Task<string?> GetToken(EntityDto entity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiHost}/Entity/GetToken?authToken={_configurationModel.ApiKey}", entity);
            if (!response.IsSuccessStatusCode) return null;
            var result = await response.Content.ReadAsStringAsync();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error getting token: {e.Message}");
        }

        return null;
    }

    public async Task<bool> UpdateEntity(EntityDto entity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiHost}/Entity?authToken={_configurationModel.ApiKey}", entity);

            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting evidence {entity.Identity}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(entity)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error updating entity: {e.Message}");
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
