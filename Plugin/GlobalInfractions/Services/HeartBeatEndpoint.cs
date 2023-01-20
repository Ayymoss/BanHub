using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class HeartBeatEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://globalinfractions.com/api/v2";
#endif


    public HeartBeatEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostInstanceHeartBeat(InstanceDto instance)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/HeartBeat/Instance", instance);
            
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
            Console.WriteLine($"[{Plugin.PluginName}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }

    public async Task<bool> PostEntityHeartBeat(List<EntityDto> entity)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/HeartBeat/Entities?authToken={_configurationModel.ApiKey}", entity);
            
            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting entity {entity.Count}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(entity)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending entity heartbeat: {e.Message}");
        }

        return false;
    }
}
