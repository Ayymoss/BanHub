using BanHub.WebCore.Shared.Models.CommunitiesView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;

public class GetCommunitiesPaginationCommand : Pagination, IRequest<PaginationContext<Shared.Models.CommunitiesView.Community>>
{
    public bool Privileged { get; set; }
}
