using System.Net;
using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;

namespace GlobalInfractions.Services;

public class InstanceEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://globalinfractions.com/api/v2";
#endif

    
    public InstanceEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostInstance(InstanceDto instance)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiHost}/Instance", instance);
            return response.IsSuccessStatusCode;
            
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting instance: {e.Message}");
        }

        return false;
    }

    public async Task<bool> IsInstanceActive(Guid guid)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiHost}/Instance/Active?guid={guid.ToString()}");
            var content = await response.Content.ReadAsStringAsync();
            _ = bool.TryParse(content, out var active);
            return response.StatusCode is HttpStatusCode.Accepted && active;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error getting instance state: {e.Message}");
        }

        return false;
    }
}
