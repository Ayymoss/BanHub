using System.Net.Http.Json;
using System.Text.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;

namespace GlobalInfractions.Services;

public class ServerEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://globalinfractions.com/api/v2";
#endif

    public ServerEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
    }

    public async Task<bool> PostServer(ServerDto server)
    {
        try
        {
            var response = await _httpClient
                .PostAsJsonAsync($"{ApiHost}/Server?authToken={_configurationModel.ApiKey}", server);
            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{Plugin.PluginName}] Error posting server {server.ServerName}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(server)}\n" +
                                  $"[{Plugin.PluginName}] End of error");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting server: {e.Message}");
        }

        return false;
    }
}
