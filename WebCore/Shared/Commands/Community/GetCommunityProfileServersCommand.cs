using BanHub.WebCore.Shared.Models.CommunityProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Community;

public class GetCommunityProfileServersCommand : IRequest<IEnumerable<Server>>
{
    public Guid Identity { get; set; }
}
