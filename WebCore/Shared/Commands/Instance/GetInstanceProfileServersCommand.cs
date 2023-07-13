using MediatR;

namespace BanHub.WebCore.Shared.Commands.Instance;

public class GetInstanceProfileServersCommand : IRequest<IEnumerable<BanHub.WebCore.Shared.Models.InstanceProfileView.Server>>
{
    public Guid Identity { get; set; }
}
