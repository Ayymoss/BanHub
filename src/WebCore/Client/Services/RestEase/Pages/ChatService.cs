using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Application.Mediatr.Chat.Commands;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class ChatService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IChatService _api = RestClient.For<IChatService>(ApiHost);
    
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

    public async Task<ChatContextRoot> GetChatContextAsync(GetMessageContextCommand chatMessageContext)
    {
        try
        {
            var response = await _api.GetChatContextAsync(chatMessageContext);
            var result = await response.DeserializeHttpResponseContentAsync<ChatContextRoot>();
            return result ?? new ChatContextRoot();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get chat: {e.Message}");
        }

        return new ChatContextRoot();
    }
}
