using MediatR;

namespace BanHub.WebCore.Shared.Commands.Instance;

public class ToggleInstanceActivationCommand : IRequest<bool>
{
    public Guid InstanceGuid { get; set; }
}
