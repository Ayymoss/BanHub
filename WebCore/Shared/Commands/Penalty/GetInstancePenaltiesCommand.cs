using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetInstancePenaltiesCommand : IRequest<IEnumerable<Shared.Models.InstanceProfileView.Penalty>>
{
    public Guid Identity { get; set; }
    public bool Privileged { get; set; }
}
