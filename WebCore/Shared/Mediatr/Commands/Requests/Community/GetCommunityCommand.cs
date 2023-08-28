using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;

public class GetCommunityCommand : IRequest<Models.CommunityProfileView.Community?>
{
    public Guid CommunityGuid { get; set; }
}
