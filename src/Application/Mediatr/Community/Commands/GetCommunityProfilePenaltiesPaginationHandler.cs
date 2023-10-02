using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityProfilePenaltiesPaginationHandler(
        IResourceQueryHelper<GetCommunityProfilePenaltiesPaginationCommand, DTOs.WebView.CommunityProfileView.Penalty> resourceQueryHelper)
    : IRequestHandler<GetCommunityProfilePenaltiesPaginationCommand, PaginationContext<DTOs.WebView.CommunityProfileView.Penalty>>
{
    public async Task<PaginationContext<DTOs.WebView.CommunityProfileView.Penalty>> Handle(
        GetCommunityProfilePenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
