using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Community;

public class ToggleCommunityActivationCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
