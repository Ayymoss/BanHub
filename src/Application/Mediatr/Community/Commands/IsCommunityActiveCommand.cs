using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class IsCommunityActiveCommand : IRequest<bool>
{
    public Guid CommunityGuid { get; set; }
}
