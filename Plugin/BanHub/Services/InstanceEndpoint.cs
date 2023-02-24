using System.Net;
using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class InstanceEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IInstanceService _api;
    private readonly ConfigurationModel _configurationModel;
    private readonly HttpClient _httpClient = new();

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{ConfigurationModel.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public InstanceEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _api = RestClient.For<IInstanceService>(ApiHost);
    }

    public async Task<bool> PostInstance(InstanceDto instance)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostInstance(instance);
                if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting instance {instance.InstanceGuid}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(instance)}\n" +
                                      $"[{ConfigurationModel.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error posting instance: {e.Message}");
        }

        return false;
    }

    public async Task<bool> IsInstanceActive(Guid guid)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.IsInstanceActive(guid.ToString());
                var content = await response.Content.ReadAsStringAsync();
                _ = bool.TryParse(content, out var active);
                return response.StatusCode is HttpStatusCode.Accepted && active;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error getting instance state: {e.Message}");
        }

        return false;
    }
}
