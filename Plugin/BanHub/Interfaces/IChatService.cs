using BanHubData.Commands.Chat;
using RestEase;

namespace BanHub.Interfaces;

public interface IChatService
{
    [Post("/Chat/AddMessages")]
    Task<HttpResponseMessage> AddInstanceChatMessagesAsync([Body] AddCommunityChatMessagesCommand messages);
}
