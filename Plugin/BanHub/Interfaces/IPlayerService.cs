using BanHubData.Commands.Player;
using RestEase;

namespace BanHub.Interfaces;

public interface IPlayerService
{
    [Get("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Query("identity")] IsPlayerBannedCommand identity);

    [Post("/Player/CreateUpdate")]
    Task<HttpResponseMessage> CreateOrUpdateAsync([Query("authToken")] string authToken, [Body] CreateOrUpdatePlayerCommand entities);

    [Post("/Player/GetToken")]
    Task<HttpResponseMessage> GetTokenAsync([Query("authToken")] string authToken, [Query] GetPlayerTokenCommand identity);
}
