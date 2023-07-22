using System.Net;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHubData.Commands.Instance;
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
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, retryCount, context) =>
        {
            Console.WriteLine(
                $"[{BanHubConfiguration.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/3)...");
        });

    public CommunityService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<ICommunityService>(ApiHost);
    }

    public async Task<bool> CreateOrUpdateCommunityAsync(CreateOrUpdateCommunityCommand community)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateCommunityAsync(community);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting community {community.CommunityGuid}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(community)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return true;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting community: {e.Message}");
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
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting community state: {e.Message}");
        }

        return false;
    }
}
