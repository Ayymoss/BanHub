using BanHub.WebCore.Shared.Commands.Players;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPlayersService
{
    [Post("/Player/Players")]
    Task<HttpResponseMessage> GetPlayersAsync([Body] GetPlayersPaginationCommand playersPagination);
}
