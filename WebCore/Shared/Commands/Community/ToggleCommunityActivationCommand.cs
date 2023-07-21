using MediatR;

namespace BanHub.WebCore.Shared.Commands.Community;

public class ToggleCommunityActivationCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
