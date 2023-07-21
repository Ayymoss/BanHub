using MediatR;

namespace BanHubData.Commands.Community;

public class IsCommunityActiveCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
