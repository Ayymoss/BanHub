using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using BanHubData.Mediatr.Commands.Requests.Community;
using Humanizer;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class CommunityService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly ICommunityService _api;
    private readonly ILogger<CommunityService> _logger;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Community API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public CommunityService(BanHubConfiguration banHubConfiguration, ILogger<CommunityService> logger, CommunitySlim communitySlim)
    {
        _logger = logger;
        _api = RestClient.For<ICommunityService>(ApiHost);
        _api.PluginVersion = communitySlim.PluginVersion;
        _api.ApiToken = banHubConfiguration.ApiKey.ToString();
    }

    public async Task<bool> CreateOrUpdateCommunityAsync(CreateOrUpdateCommunityCommand community)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateCommunityAsync(community);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Error posting community {CommunityGuid} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid}, JSON: {@Community}",
                        community.CommunityGuid, response.StatusCode, response.ReasonPhrase, body, community);
                }

                return true;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"[{BanHubConfiguration.Name}] Error posting community: {e.Message} - JSON: {JsonSerializer.Serialize(community)}");
        }

        return false;
    }

    public async Task<bool> IsCommunityActiveAsync(string guid)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsCommunityActiveAsync(guid);
                return response.StatusCode is HttpStatusCode.Accepted;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting community active state: {e.Message}");
        }

        return false;
    }
}
