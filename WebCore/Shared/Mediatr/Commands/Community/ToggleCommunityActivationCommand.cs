using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Community;

public class ToggleCommunityActivationCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
