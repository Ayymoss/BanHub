using BanHub.Application.Mediatr.Player.Commands;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPlayersService
{
    [Post("/Player/Pagination")]
    Task<HttpResponseMessage> GetPlayersAsync([Body] GetPlayersPaginationCommand playersPagination);
}
