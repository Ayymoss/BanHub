using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class ChatService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IChatService _api;

    public ChatService()
    {
        _api = RestClient.For<IChatService>(ApiHost);
    }

    public async Task<ChatContext> GetChatPaginationAsync(GetChatPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetChatPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<ChatContext>();
            return result ?? new ChatContext();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get chat: {e.Message}");
        }

        return new ChatContext();
    }

    public async Task<int> GetChatCountAsync(string identity)
    {
        try
        {
            var response = await _api.GetChatCountAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<ChatCount>();
            return result?.Count ?? 0;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get chat: {e.Message}");
        }

        return 0;
    }
}
