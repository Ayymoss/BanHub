using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunitiesPaginationHandler(
        IResourceQueryHelper<GetCommunitiesPaginationCommand, DTOs.WebView.CommunitiesView.Community> resourceQueryHelper)
    : IRequestHandler<GetCommunitiesPaginationCommand, PaginationContext<DTOs.WebView.CommunitiesView.Community>>
{
    public async Task<PaginationContext<DTOs.WebView.CommunitiesView.Community>> Handle(GetCommunitiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
