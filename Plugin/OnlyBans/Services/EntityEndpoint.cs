using System.Net;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using Data.Commands.Player;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class EntityEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPlayerService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{BanHubConfiguration.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public EntityEndpoint(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IPlayerService>(ApiHost);
    }

    /// <summary>
    /// Get player state from BanHub master
    /// </summary>
    /// <param name="identity"></param>
    /// <returns>True if Banned, False if Not</returns>
    public async Task<bool> IsPlayerBannedAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsPlayerBannedAsync(identity);
                if (!response.IsSuccessStatusCode) return false;
                return response.StatusCode is HttpStatusCode.Forbidden;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting player ban state: {e.Message}");
        }

        return false;
    }

    public async Task<string?> GetTokenAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.GetTokenAsync(_banHubConfiguration.ApiKey.ToString(), identity);
                if (!response.IsSuccessStatusCode) return null;
                var result = await response.Content.ReadAsStringAsync();
                return string.IsNullOrEmpty(result) ? null : result;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting token: {e.Message}");
        }

        return null;
    }

    public async Task<string?> UpdateEntityAsync(CreateOrUpdatePlayerCommand entity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.UpdateEntityAsync(entity, _banHubConfiguration.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting evidence {entity.PlayerIdentity}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(entity)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error updating entity: {e.Message}");
        }

        return null;
    }
}
