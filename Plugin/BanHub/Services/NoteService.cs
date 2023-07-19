using System.Net;
using BanHub.Configuration;
using BanHub.Interfaces;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class NoteService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly INoteService _api;
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

    public NoteService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<INoteService>(ApiHost);
    }

    public async Task<int> GetUserNotesCountAsync(string identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.GetUserNotesCountAsync(identity);
                if (!response.IsSuccessStatusCode) return 0;
                var count = await response.Content.ReadAsStringAsync();
                return int.TryParse(count, out var result) ? result : 0;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting instance state: {e.Message}");
        }

        return 0;
    }
}
