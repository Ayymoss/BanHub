using BanHub.Domain.ValueObjects;
using BanHub.Domain.ValueObjects.Plugin;
using RestEase;

namespace BanHub.Plugin.Interfaces;

public interface IPlayerService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Body] IsPlayerBannedCommandSlim identity);

    [Post("/Player")]
    Task<HttpResponseMessage> CreateOrUpdatePlayerAsync([Body] CreateOrUpdatePlayerNotificationSlim entities);

    [Get("/Player/GetToken/{identity}")]
    Task<HttpResponseMessage> GetTokenAsync([Path("identity")] string identity);

    [Get("/Player/HasIdentityBan/{identity}")]
    Task<HttpResponseMessage> HasIdentityBanAsync([Path("identity")] string identity);
}
