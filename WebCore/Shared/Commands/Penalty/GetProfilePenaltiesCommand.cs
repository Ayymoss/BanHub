using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetProfilePenaltiesCommand : IRequest<IEnumerable<Models.PlayerProfileView.Penalty>>
{
    public string Identity { get; set; }
    public bool Privileged { get; set; }
}
