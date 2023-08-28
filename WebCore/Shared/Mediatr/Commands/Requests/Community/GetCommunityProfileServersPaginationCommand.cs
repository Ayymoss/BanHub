using BanHub.WebCore.Shared.Models.CommunityProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;

public class GetCommunityProfileServersPaginationCommand : Pagination, IRequest<PaginationContext<Server>>
{
    public Guid Identity { get; set; }
}
