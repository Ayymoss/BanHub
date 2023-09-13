using BanHubData.Mediatr.Commands.Events.Player;
using BanHubData.Mediatr.Commands.Requests.Player;
using RestEase;

namespace BanHub.Interfaces;

public interface IPlayerService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Body] IsPlayerBannedCommand identity);

    [Post("/Player")]
    Task<HttpResponseMessage> CreateOrUpdatePlayerAsync([Body] CreateOrUpdatePlayerNotification entities);

    [Get("/Player/GetToken/{identity}")]
    Task<HttpResponseMessage> GetTokenAsync([Path("identity")] string identity);

    [Get("/Player/HasIdentityBan/{identity}")]
    Task<HttpResponseMessage> HasIdentityBanAsync([Path("identity")] string identity);
}
