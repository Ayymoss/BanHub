using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;

public class GetCommunityProfilePenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<Models.CommunityProfileView.Penalty>>
{
    public Guid Identity { get; set; }
    public bool Privileged { get; set; }
}
