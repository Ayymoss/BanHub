using BanHub.WebCore.Shared.Models.PlayersView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Players;

public class GetPlayersPaginationCommand : Pagination, IRequest<PlayerContext>
{
    
}


