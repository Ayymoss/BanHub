using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
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

    public async Task<PaginationContext<Chat>> GetChatPaginationAsync(GetChatPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetChatPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Chat>>();
            return result ?? new PaginationContext<Chat>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get chat: {e.Message}");
        }

        return new PaginationContext<Chat>();
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
