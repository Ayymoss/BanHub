using System.Net.Http.Json;
using GlobalInfractions.Configuration;
using GlobalInfractions.Models;

namespace GlobalInfractions.Services;

public class ServerEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123";
#else
    private const string ApiHost = "https://globalinfractions.com";
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
                .PostAsJsonAsync($"{ApiHost}/api/Infraction?authToken={_configurationModel.ApiKey}", server);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error posting server: {e.Message}");
        }

        return false;
    }

    public async Task<ServerDto?> GetEntity(string serverId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiHost}/api/Server?serverId={serverId}");

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<ServerDto>();
            return json;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{Plugin.PluginName}] Error getting server: {e.Message}");
        }

        return null;
    }
}
