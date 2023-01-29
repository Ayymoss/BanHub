using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Models;

namespace BanHub.Services;

public class PenaltyEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://banhub.gg/api/v2";
#endif

    public PenaltyEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<(bool, Guid?)> PostPenalty(PenaltyDto penalty)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/Penalty?authToken={_configurationModel.ApiKey}", penalty);
            var preGuid = await response.Content.ReadAsStringAsync();
            var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting penalty {penalty.Reason}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {preGuid}\nJSON: {JsonSerializer.Serialize(penalty)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
            return (response.IsSuccessStatusCode && parsedState, guid);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting penalty: {e.Message}");
        }

        return (false, null);
    }

    public async Task<bool> SubmitEvidence(PenaltyDto penalty)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/Penalty/Evidence?authToken={_configurationModel.ApiKey}", penalty);
            
            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting evidence {penalty.Evidence}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(penalty)}\n" +
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
