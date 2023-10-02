using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityProfileServersPaginationHandler(
        IResourceQueryHelper<GetCommunityProfileServersPaginationCommand, DTOs.WebView.CommunityProfileView.Server> resourceQueryHelper)
    : IRequestHandler<GetCommunityProfileServersPaginationCommand, PaginationContext<DTOs.WebView.CommunityProfileView.Server>>
{
    public async Task<PaginationContext<DTOs.WebView.CommunityProfileView.Server>> Handle(
        GetCommunityProfileServersPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
