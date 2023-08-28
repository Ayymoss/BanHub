using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Community;

public class IsCommunityActiveCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
