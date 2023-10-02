using BanHub.Domain.ValueObjects.Plugin;
using RestEase;

namespace BanHub.Plugin.Interfaces;

public interface IChatService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Chat/AddMessages")]
    Task<HttpResponseMessage> AddInstanceChatMessagesAsync([Body] AddCommunityChatMessagesNotificationSlim messages);
}
