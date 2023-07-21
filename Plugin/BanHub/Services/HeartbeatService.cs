using System.Text.Json;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHubData.Commands.Heartbeat;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class HeartbeatService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IHeartbeatService _api;
    private readonly BanHubConfiguration _banHubConfiguration;
    
    /*
     *System.Collections.Generic.KeyNotFoundException: The given key 'retryCount' was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at Polly.Context.get_Item(String key)
   at BanHub.Services.HeartbeatService.<>c.<.ctor>b__4_1(Exception exception, TimeSpan retryCount, Context context)
   at Polly.AsyncRetrySyntax.<>c__DisplayClass22_0.<<WaitAndRetryAsync>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at Polly.Retry.AsyncRetryEngine.ImplementationAsync[TResult](Func`3 action, Context context, CancellationToken cancellationToken, ExceptionPredicates shouldRetryExceptionPredicates, ResultPredicates`1 shouldRetryResultPredicates, Func`5 onRetryAsync, Int32 permittedRetryCount, IEnumerable`1 sleepDurationsEnumerable, Func`4 sleepDurationProvider, Boolean continueOnCapturedContext)
   at Polly.AsyncPolicy.ExecuteAsync[TResult](Func`3 action, Context context, CancellationToken cancellationToken, Boolean continueOnCapturedContext)
   at BanHub.Services.HeartbeatService.PostInstanceHeartBeat(InstanceHeartbeatCommand instance)
   at BanHub.Managers.HeartBeatManager.InstanceHeartBeat()

     * 
     */

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{BanHubConfiguration.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/{context["retryCount"]})...");
            });

    public HeartbeatService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IHeartbeatService>(ApiHost);
    }

    public async Task<bool> PostCommunityHeartbeat(CommunityHeartbeatCommand community)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostCommunityHeartBeatAsync(community);
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting instance heartbeat.\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(community)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending instance heartbeat: {e.Message}");
        }

        return false;
    }

    public async Task<bool> PostEntityHeartbeat(PlayersHeartbeatCommand players)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.PostPlayersHeartBeatAsync(players, _banHubConfiguration.ApiKey.ToString());
                if (!response.IsSuccessStatusCode && _banHubConfiguration.DebugMode)
                {
                    Console.WriteLine($"\n[{BanHubConfiguration.Name}] Error posting entity heartbeat.\n" +
                                      $"SC: {response.StatusCode}\n" +
                                      $"RP: {response.ReasonPhrase}\n" +
                                      $"B: {await response.Content.ReadAsStringAsync()}\n" +
                                      $"JSON: {JsonSerializer.Serialize(players)}\n" +
                                      $"[{BanHubConfiguration.Name}] End of error");
                }

                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending entity heartbeat: {e.Message}");
        }

        return false;
    }
}
