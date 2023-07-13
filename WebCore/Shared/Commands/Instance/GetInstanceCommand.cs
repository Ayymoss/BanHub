using MediatR;

namespace BanHub.WebCore.Shared.Commands.Instance;

public class GetInstanceCommand : IRequest<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance?>
{
    public string InstanceGuid { get; set; }
}
