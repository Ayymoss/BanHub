using System.Net;
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

public class PenaltyService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IPenaltyService _api;
    private readonly ILogger<PenaltyService> _logger;

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine(
                    $"[{BanHubConfiguration.Name}] Penalty API: {exception.Message}. " +
                    $"Retrying in {retryDelay.Humanize()}...");
            });

    public PenaltyService(BanHubConfiguration banHubConfiguration, ILogger<PenaltyService> logger, CommunitySlim communitySlim)
    {
        _logger = logger;
        _api = RestClient.For<IPenaltyService>(ApiHost);
        _api.PluginVersion = communitySlim.PluginVersion;
        _api.ApiToken = banHubConfiguration.ApiKey.ToString();
    }

    public async Task<(bool, Guid?)> AddPlayerPenaltyAsync(AddPlayerPenaltyCommandSlim penalty)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.AddPlayerPenaltyAsync(penalty);
                var preGuid = await response.Content.ReadAsStringAsync();
                var parsedState = Guid.TryParse(preGuid.Replace("\"", ""), out var guid);

                if (response.StatusCode is HttpStatusCode.BadRequest)
                {
                    _logger.LogError("Error posting penalty {PenaltyReason} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid} JSON: {@Penalty}",
                        penalty.Reason, response.StatusCode, response.ReasonPhrase, preGuid, penalty);
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

    public async Task<bool> SubmitEvidence(AddPlayerPenaltyEvidenceCommandSlim penalty)
    {
        try
        {
            var response = await _api.AddPlayerPenaltyEvidenceAsync(penalty);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error posting evidence {PenaltyEvidence} SC: {StatusCode} RP: {ReasonPhrase} B: {Guid} JSON: {@Penalty}",
                    penalty.Evidence, response.StatusCode, response.ReasonPhrase, body, penalty);
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
