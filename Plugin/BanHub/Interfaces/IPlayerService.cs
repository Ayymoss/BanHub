using BanHubData.Commands.Player;
using RestEase;

namespace BanHub.Interfaces;

public interface IPlayerService
{
    [Post("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Body] IsPlayerBannedCommand identity);

    [Post("/Player")]
    Task<HttpResponseMessage> CreateOrUpdatePlayerAsync([Query("authToken")] string authToken, [Body] CreateOrUpdatePlayerCommand entities);

    [Post("/Player/GetToken/{identity}")]
    Task<HttpResponseMessage> GetTokenAsync([Query("authToken")] string authToken, [Path("identity")] string identity);
}
