using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Models;

namespace BanHub.Services;

public class InstanceEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://banhub.gg/api/v2";
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
            
            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting instance {instance.InstanceGuid}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(instance)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
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
