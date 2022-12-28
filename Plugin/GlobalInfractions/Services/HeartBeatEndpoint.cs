using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class HeartBeatEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
    private const string ApiHost = "http://localhost:5000";

    public HeartBeatEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostInstanceHeartBeat(InstanceDto instance)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/api/HeartBeat/Instance", instance);
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
                .PostAsJsonAsync($"{ApiHost}/api/HeartBeat/Entities?authToken={_configurationModel.ApiKey}", entity);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error sending entity heartbeat: {e.Message}");
        }

        return false;
    }
}
