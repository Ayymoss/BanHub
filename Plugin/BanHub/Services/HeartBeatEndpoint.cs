using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Models;

namespace BanHub.Services;

public class HeartBeatEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://banhub.gg/api/v2";
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
                Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting instance {instance.InstanceGuid}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(instance)}\n" +
                                  $"[{ConfigurationModel.Name}] End of error");
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error sending instance heartbeat: {e.Message}");
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
                Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting entity {entity.Count}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(entity)}\n" +
                                  $"[{ConfigurationModel.Name}] End of error");
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error sending entity heartbeat: {e.Message}");
        }

        return false;
    }
}
