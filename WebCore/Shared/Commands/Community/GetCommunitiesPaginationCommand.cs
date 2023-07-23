using BanHub.WebCore.Shared.Models.CommunitiesView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Community;

public class GetCommunitiesPaginationCommand : Pagination, IRequest<CommunityContext>
{
    public bool Privileged { get; set; }
}
