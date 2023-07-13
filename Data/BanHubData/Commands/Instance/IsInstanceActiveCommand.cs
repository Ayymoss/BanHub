using MediatR;

namespace BanHubData.Commands.Instance;

public class IsInstanceActiveCommand : IRequest<bool>
{
    public Guid InstanceGuid { get; set; }
}
