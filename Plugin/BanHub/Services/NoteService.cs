using System.Net;
using System.Net.Sockets;
using BanHub.Configuration;
using BanHub.Interfaces;
using Humanizer;
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

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Note API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
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
        catch (Exception e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error getting instance state: {e.Message}");
        }

        return 0;
    }
}
