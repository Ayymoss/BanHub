using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Utilities;
using BanHubData.Commands.Heartbeat;
using Humanizer;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class HeartbeatService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IHeartbeatService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e =>
            e.InnerException is TimeoutException || e.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Heartbeat API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });
    
    public HeartbeatService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IHeartbeatService>(ApiHost);
    }

    public async Task<bool> PostCommunityHeartbeat(CommunityHeartbeatCommand community)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostCommunityHeartBeatAsync(community);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting instance heartbeat.\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(community)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending community heartbeat: {e.Message}");
        }

        return false;
    }

    public async Task<bool> PostEntityHeartbeat(PlayersHeartbeatCommand players)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostPlayersHeartBeatAsync(players, _banHubConfiguration.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting entity heartbeat.\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(players)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending entity heartbeat: {errorMessage}");
        }

        return false;
    }
}
