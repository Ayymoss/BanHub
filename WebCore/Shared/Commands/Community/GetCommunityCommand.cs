using MediatR;

namespace BanHub.WebCore.Shared.Commands.Community;

public class GetCommunityCommand : IRequest<Models.CommunityProfileView.Community?>
{
    public Guid CommunityGuid { get; set; }
}
