using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHubData.Commands.Instance.Server;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IServerService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending heartbeat: {exception.Message}. " +
                                  $"Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public ServerService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IServerService>(ApiHost);
    }

    public async Task<bool> PostServer(CreateOrUpdateServerCommand server)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateServerAsync(_banHubConfiguration.ApiKey.ToString(), server);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting server {server.ServerName}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(server)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting server: {e.Message}");
        }

        return false;
    }
}
