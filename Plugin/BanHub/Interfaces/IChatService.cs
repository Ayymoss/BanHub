using BanHubData.Mediatr.Commands.Requests.Chat;
using RestEase;

namespace BanHub.Interfaces;

public interface IChatService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Chat/AddMessages")]
    Task<HttpResponseMessage> AddInstanceChatMessagesAsync([Body] AddCommunityChatMessagesCommand messages);
}
