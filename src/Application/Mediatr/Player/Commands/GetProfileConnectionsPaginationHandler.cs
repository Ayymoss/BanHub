using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileConnectionsPaginationHandler(IResourceQueryHelper<GetProfileConnectionsPaginationCommand, Connection> queryHelper)
    : IRequestHandler<GetProfileConnectionsPaginationCommand, PaginationContext<Connection>>
{
    public async Task<PaginationContext<Connection>> Handle(GetProfileConnectionsPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await queryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
