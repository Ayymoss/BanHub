using BanHubData.Mediatr.Commands.Requests.Chat;
using RestEase;

namespace BanHub.Interfaces;

public interface IChatService
{
    [Post("/Chat/AddMessages")]
    Task<HttpResponseMessage> AddInstanceChatMessagesAsync([Body] AddCommunityChatMessagesCommand messages, [Query("authToken")] string authToken);
}
