using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class GetProfilePenaltiesPaginationHandler(
        IResourceQueryHelper<GetProfilePenaltiesPaginationCommand, DTOs.WebView.PlayerProfileView.Penalty> resourceQueryHelper)
    : IRequestHandler<GetProfilePenaltiesPaginationCommand, PaginationContext<DTOs.WebView.PlayerProfileView.Penalty>>
{
    public async Task<PaginationContext<DTOs.WebView.PlayerProfileView.Penalty>> Handle(GetProfilePenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
