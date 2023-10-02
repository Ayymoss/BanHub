using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class ToggleCommunityActivationCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
