using System.Net.Sockets;
using BanHub.Configuration;
using BanHub.Interfaces;
using BanHub.Models;
using BanHub.Utilities;
using BanHubData.Mediatr.Commands.Requests.Chat;
using Humanizer;
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

    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<HttpRequestException>(e => e.InnerException is SocketException)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, retryDelay, context) =>
            {
                Console.WriteLine($"[{BanHubConfiguration.Name}] Chat API: {exception.Message}. " +
                                  $"Retrying in {retryDelay.Humanize()}...");
            });

    public ChatService(BanHubConfiguration banHubConfiguration, CommunitySlim communitySlim)
    {
        _api = RestClient.For<IChatService>(ApiHost);
        _api.PluginVersion = communitySlim.PluginVersion;
        _api.ApiToken = banHubConfiguration.ApiKey.ToString();
    }

    public async Task<bool> AddCommunityChatMessagesAsync(AddCommunityChatMessagesCommand identity)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _api.AddInstanceChatMessagesAsync(identity);
                return response.IsSuccessStatusCode;
            });
        }
        catch (Exception e)
        {
            var errorMessage = HelperMethods.ObscureGuid(e.Message);
            Console.WriteLine($"[{BanHubConfiguration.Name}] Error sending chat messages: {errorMessage}");
        }

        return false;
    }
}
