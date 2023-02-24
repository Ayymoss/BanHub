using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class PenaltyEndpoint
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPenaltyService _api;
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


    public PenaltyEndpoint(ConfigurationModel configurationModel)
    {
        _configurationModel = configurationModel;
        _api = RestClient.For<IPenaltyService>(ApiHost);
    }

    public async Task<(bool, Guid?)> PostPenalty(PenaltyDto penalty)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostPenalty(penalty, _configurationModel.ApiKey.ToString());
                var preGuid = await response.Content.ReadAsStringAsync();
                var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

                if ((!response.IsSuccessStatusCode && _configurationModel.DebugMode) || !parsedState)
                {
                    Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting penalty {penalty.Reason}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {preGuid}\n" +
                                      $"JSON: {JsonSerializer.Serialize(penalty)}\n" +
                                      $"[{ConfigurationModel.Name}] End of error");
                }

                return (response.IsSuccessStatusCode && parsedState, guid);
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error posting penalty: {e.Message}");
        }

        return (false, null);
    }

    public async Task<bool> SubmitEvidence(PenaltyDto penalty)
    {
        try
        {
            var response = await _api.SubmitEvidence(penalty, _configurationModel.ApiKey.ToString());

            if (!response.IsSuccessStatusCode && _configurationModel.DebugMode)
            {
                Console.WriteLine($"\n[{ConfigurationModel.Name}] Error posting evidence {penalty.Evidence}\n" +
                                  $"SC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\n" +
                                  $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                  $"JSON: {JsonSerializer.Serialize(penalty)}\n" +
                                  $"[{ConfigurationModel.Name}] End of error");
            }

            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{ConfigurationModel.Name}] Error submitting information: {e.Message}");
        }

        return false;
    }
}
