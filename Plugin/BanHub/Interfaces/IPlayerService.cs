using BanHubData.Commands.Player;
using RestEase;

namespace BanHub.Interfaces;

public interface IPlayerService
{
    [Post("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Query("authToken")] string authToken, [Body] IsPlayerBannedCommand identity);

    [Post("/Player")]
    Task<HttpResponseMessage> CreateOrUpdatePlayerAsync([Query("authToken")] string authToken, [Body] CreateOrUpdatePlayerCommand entities);

    [Get("/Player/GetToken/{identity}")]
    Task<HttpResponseMessage> GetTokenAsync([Query("authToken")] string authToken, [Path("identity")] string identity);

    [Get("/Player/HasIdentityBan/{identity}")]
    Task<HttpResponseMessage> HasIdentityBanAsync([Path("identity")] string identity);
}
