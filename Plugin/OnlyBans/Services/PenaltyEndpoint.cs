using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using Data.Domains;
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


    public PenaltyEndpoint(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IPenaltyService>(ApiHost);
    }

    public async Task<(bool, Guid?)> PostPenalty(Penalty penalty)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostPenalty(penalty, _banHubConfiguration.ApiKey.ToString());
                var preGuid = await response.Content.ReadAsStringAsync();
                var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

                if ((!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode) || !parsedState)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting penalty {penalty.Reason}\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {preGuid}\n" +
                                      $"JSON: {JsonSerializer.Serialize(penalty)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return (response.IsSuccessStatusCode && parsedState, guid);
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting penalty: {e.Message}");
        }

        return (false, null);
    }

    public async Task<bool> SubmitEvidence(Penalty penalty)
    {
        try
        {
            var response = await _api.SubmitEvidence(penalty, _banHubConfiguration.ApiKey.ToString());

            if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
            {
                Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting evidence {penalty.Evidence}\n" +
                                  $"SC: {response.StatusCode}\n" +
                                  $"RP: {response.ReasonPhrase}\n" +
                                  $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                  $"JSON: {JsonSerializer.Serialize(penalty)}\n" +
                                  $"[{BanHubConfiguration.Name}] End of error");
            }

            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error submitting information: {e.Message}");
        }

        return false;
    }
}
