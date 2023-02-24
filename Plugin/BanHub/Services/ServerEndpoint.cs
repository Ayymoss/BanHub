using System.Net.Http.Json;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class ServerEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IServerService _api;
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine($"[{ConfigurationModel.Name}] Error sending heartbeat: {exception.Message}. " +
                                  $"Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public ServerEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _api = RestClient.For<IServerService>(ApiHost);
    }

    public async Task<bool> PostServer(ServerDto server)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostServer(server, _configurationModel.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting server {server.ServerName}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(server)}\n" +
                                      $"[{ConfigurationModel.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error posting server: {e.Message}");
        }

        return false;
    }
}
