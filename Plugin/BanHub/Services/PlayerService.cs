using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Utilities;
using BanHubData.Mediatr.Commands.Events.Player;
using BanHubData.Mediatr.Commands.Requests.Player;
using Humanizer;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestEase;
using Serilog;

namespace BanHub.Services;

public class PlayerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPlayerService _api;
    private readonly BanHubConfiguration _banHubConfiguration;
    private readonly ILogger<PlayerService> _logger;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Player API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public PlayerService(BanHubConfiguration banHubConfiguration, ILogger<PlayerService> logger)
    {
        _banHubConfiguration = banHubConfiguration;
        _logger = logger;
        _api = RestClient.For<IPlayerService>(ApiHost);
    }

    public async Task<bool> IsPlayerBannedAsync(IsPlayerBannedCommand identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsPlayerBannedAsync(_banHubConfiguration.ApiKey.ToString(), identity);
                return response.StatusCode is HttpStatusCode.Unauthorized;
            });
        }
        catch (Exception e)
        {
            if (e is ApiException {StatusCode: HttpStatusCode.Unauthorized}) return true;
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting player ban state: {e.Message}");
        }

        return false;
    }

    public async Task<bool> IsPlayerBannedAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.HasIdentityBanAsync(identity);
                return response.StatusCode is HttpStatusCode.Unauthorized;
            });
        }
        catch (Exception e)
        {
            if (e is ApiException {StatusCode: HttpStatusCode.Unauthorized}) return true;
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
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting token: {errorMessage}");
        }

        return null;
    }

    public async Task<bool> CreateOrUpdatePlayerAsync(CreateOrUpdatePlayerNotification player)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdatePlayerAsync(_banHubConfiguration.ApiKey.ToString(), player);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error creating or updating player {PlayerIdentity} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid}",
                        player.PlayerIdentity, response.StatusCode, response.ReasonPhrase, body);
                    return false;
                }

                return true;
            });
        }
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error updating entity: {errorMessage}");
        }

        return false;
    }
}
