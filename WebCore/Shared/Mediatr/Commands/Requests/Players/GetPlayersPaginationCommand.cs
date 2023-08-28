using BanHub.WebCore.Shared.Models.PlayersView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Players;

public class GetPlayersPaginationCommand : Pagination, IRequest<PaginationContext<Player>>
{
}
