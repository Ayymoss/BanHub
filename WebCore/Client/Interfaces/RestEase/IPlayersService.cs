using BanHub.WebCore.Shared.Mediatr.Commands.Players;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPlayersService
{
    [Post("/Player/Pagination")]
    Task<HttpResponseMessage> GetPlayersAsync([Body] GetPlayersPaginationCommand playersPagination);
}
