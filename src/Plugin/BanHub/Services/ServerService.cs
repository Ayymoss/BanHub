using System.Net.Sockets;
using BanHub.Domain.ValueObjects.Plugin;
using BanHub.Plugin.Configuration;
using BanHub.Plugin.Interfaces;
using BanHub.Plugin.Models;
using BanHub.Plugin.Utilities;
using Humanizer;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Plugin.Services;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IServerService _api;
    private readonly ILogger<ServerService> _logger;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Server API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public ServerService(BanHubConfiguration banHubConfiguration, ILogger<ServerService> logger, CommunitySlim communitySlim)
    {
        _logger = logger;
        _api = RestClient.For<IServerService>(ApiHost);
        _api.PluginVersion = communitySlim.PluginVersion;
        _api.ApiToken = banHubConfiguration.ApiKey.ToString();
    }

    public async Task<bool> PostServer(CreateOrUpdateServerCommandSlim server)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateServerAsync(server);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error posting server {PenaltyReason} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid}",
                        server.ServerName, response.StatusCode, response.ReasonPhrase, body);
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting server: {errorMessage}");
        }

        return false;
    }
}
