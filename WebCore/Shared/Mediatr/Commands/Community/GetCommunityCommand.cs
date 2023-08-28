using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Community;

public class GetCommunityCommand : IRequest<Models.CommunityProfileView.Community?>
{
    public Guid CommunityGuid { get; set; }
}
