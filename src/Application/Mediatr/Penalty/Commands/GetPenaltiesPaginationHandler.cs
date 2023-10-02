using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class GetPenaltiesPaginationHandler(
        IResourceQueryHelper<GetPenaltiesPaginationCommand, DTOs.WebView.PenaltiesView.Penalty> resourceQueryHelper)
    : IRequestHandler<GetPenaltiesPaginationCommand, PaginationContext<DTOs.WebView.PenaltiesView.Penalty>>
{
    public async Task<PaginationContext<DTOs.WebView.PenaltiesView.Penalty>> Handle(GetPenaltiesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
