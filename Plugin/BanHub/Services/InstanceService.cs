using System.Net;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHubData.Commands.Instance;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class InstanceService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IInstanceService _api;
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

    public InstanceService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IInstanceService>(ApiHost);
    }

    public async Task<bool> PostInstance(CreateOrUpdateInstanceCommand instance)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.CreateOrUpdateInstanceAsync(instance);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting instance {instance.InstanceGuid}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(instance)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting instance: {e.Message}");
        }

        return false;
    }

    public async Task<bool> IsInstanceActive(string guid)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsInstanceActiveAsync(guid);
                return response.StatusCode is HttpStatusCode.Accepted;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting instance state: {e.Message}");
        }

        return false;
    }
}
