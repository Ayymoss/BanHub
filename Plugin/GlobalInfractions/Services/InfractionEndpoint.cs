using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;

namespace GlobalInfractions.Services;

public class InfractionEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://globalinfractions.com/api/v2";
#endif


    public InfractionEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<(bool, Guid?)> PostInfraction(InfractionDto infraction)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/Infraction?authToken={_configurationModel.ApiKey}", infraction);
            var preGuid = await response.Content.ReadAsStringAsync();
            var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting infraction {infraction.Reason}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {preGuid}\nJSON: {JsonSerializer.Serialize(infraction)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
            return (response.IsSuccessStatusCode && parsedState, guid);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting infraction: {e.Message}");
        }

        return (false, null);
    }

    public async Task<bool> SubmitEvidence(InfractionDto infraction)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/Infraction/Evidence?authToken={_configurationModel.ApiKey}", infraction);
            
            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting evidence {infraction.Evidence}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(infraction)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error submitting information: {e.Message}");
        }

        return false;
    }
}
