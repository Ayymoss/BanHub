using BanHub.Domain.Interfaces.Repositories.Pagination;
using PlayerView = BanHub.Application.DTOs.WebView.PlayersView.Player;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetPlayersPaginationHandler(IResourceQueryHelper<GetPlayersPaginationCommand, PlayerView> queryHelper)
    : IRequestHandler<GetPlayersPaginationCommand, PaginationContext<PlayerView>>
{
    public async Task<PaginationContext<PlayerView>> Handle(GetPlayersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await queryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
