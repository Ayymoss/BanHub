using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using BanHub.Commands;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Utilities;
using BanHubData.Commands.Penalty;
using Humanizer;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class PenaltyService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPenaltyService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine(
                    $"[{BanHubConfiguration.Name}] Penalty API: {exception.Message}. " +
                    $"Retrying in {retryDelay.Humanize()}...");
            });

    public PenaltyService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IPenaltyService>(ApiHost);
    }

    public async Task<(bool, Guid?)> AddPlayerPenaltyAsync(AddPlayerPenaltyCommand penalty)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.AddPlayerPenaltyAsync(_banHubConfiguration.ApiKey.ToString(), penalty);
                var preGuid = await response.Content.ReadAsStringAsync();
                var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

                if (response.StatusCode is HttpStatusCode.BadRequest && _banHubConfiguration.DebugMode)
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
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error posting penalty: {errorMessage}");
        }

        return (false, null);
    }

    public async Task<bool> SubmitEvidence(AddPlayerPenaltyEvidenceCommand penalty)
    {
        try
        {
            var response = await _api.AddPlayerPenaltyEvidenceAsync(_banHubConfiguration.ApiKey.ToString(), penalty);

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
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error submitting information: {errorMessage}");
        }

        return false;
    }
}
