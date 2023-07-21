using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetCommunityPenaltiesCommand : IRequest<IEnumerable<Models.CommunityProfileView.Penalty>>
{
    public Guid Identity { get; set; }
    public bool Privileged { get; set; }
}
