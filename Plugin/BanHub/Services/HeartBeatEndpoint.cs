using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using Polly;
using Polly.Retry;
using RestEase;
using BanHub.Models;

namespace BanHub.Services;

public class HeartBeatEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IHeartBeatService _api;
    private readonly ConfigurationModel _configurationModel;

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{ConfigurationModel.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public HeartBeatEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _api = RestClient.For<IHeartBeatService>(ApiHost);
    }

    public async Task<bool> PostInstanceHeartBeat(InstanceDto instance)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostInstanceHeartBeat(instance);
                if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting instance heartbeat.\n" +
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
            Console.WriteLine($"[{ConfigurationModel.Name}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }

    public async Task<bool> PostEntityHeartBeat(List<EntityDto> entities)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostEntityHeartBeat(entities, _configurationModel.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting entity heartbeat.\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(entities)}\n" +
                                      $"[{ConfigurationModel.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error sending entity heartbeat: {e.Message}");
        }

        return false;
    }
}
