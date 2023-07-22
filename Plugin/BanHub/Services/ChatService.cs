using BanHub.Configuration;
using BanHub.Interfaces;
using BanHubData.Commands.Chat;
using Polly;
using Polly.Retry;
using RestEase;

namespace BanHub.Services;

public class ChatService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif

    private readonly IChatService _api;
    private readonly BanHubConfiguration _banHubConfiguration;

    private readonly AsyncRetryPolicy _retryPolicy = Policy.Handle<HttpRequestException>().Or<ApiException>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryCount, context) =>
            {
                Console.WriteLine(
                    $"[{BanHubConfiguration.Name}] Error sending heartbeat: {exception.Message}. Retrying ({retryCount}/3)...");
            });

    public ChatService(BanHubConfiguration banHubConfiguration)
    {
        _banHubConfiguration = banHubConfiguration;
        _api = RestClient.For<IChatService>(ApiHost);
    }

    public async Task<bool> AddInstanceChatMessagesAsync(AddCommunityChatMessagesCommand identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.AddInstanceChatMessagesAsync(identity);
                return response.IsSuccessStatusCode;
            });
        }
        catch (ApiException e)
        {
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending chat messages: {e.Message}");
        }

        return false;
    }
}
