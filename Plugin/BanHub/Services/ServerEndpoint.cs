using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Models;

namespace BanHub.Services;

public class ServerEndpoint
{
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api/v2";
#else
    private const string ApiHost = "https://banhub.gg/api/v2";
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
                Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting server {server.ServerName}\nSC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\nB: {await response.Content.ReadAsStringAsync()}\nJSON: {JsonSerializer.Serialize(server)}\n" +
                                  $"[{ConfigurationModel.Name}] End of error");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error posting server: {e.Message}");
        }

        return false;
    }
}
