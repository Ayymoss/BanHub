using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityProfileServersPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.CommunityProfileView.Server>>
{
    public Guid Identity { get; set; }
}
