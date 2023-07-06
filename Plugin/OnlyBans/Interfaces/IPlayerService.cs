using Data.Commands.Player;
using RestEase;

namespace BanHub.Interfaces;

public interface IPlayerService
{
    [Get("/Player/IsBanned")]
    Task<HttpResponseMessage> IsPlayerBannedAsync([Query("identity")] IsPlayerBannedCommand identity);

    [Post("/Player")]
    Task<HttpResponseMessage> UpdateEntityAsync([Body] CreateOrUpdatePlayerCommand entities, [Query("authToken")] string authToken);

    [Post("/Player/GetToken")]
    Task<HttpResponseMessage> GetTokenAsync([Query("authToken")] string authToken, [Query] GetPlayerTokenCommand identity);
}
