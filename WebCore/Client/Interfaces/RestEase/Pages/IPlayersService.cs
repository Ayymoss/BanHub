using BanHub.WebCore.Shared.Commands.Players;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPlayersService
{
    [Post("/Player/Pagination")]
    Task<HttpResponseMessage> GetPlayersAsync([Body] GetPlayersPaginationCommand playersPagination);
}
