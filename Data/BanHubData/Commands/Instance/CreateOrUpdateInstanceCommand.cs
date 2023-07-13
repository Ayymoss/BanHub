using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Instance;

public class CreateOrUpdateInstanceCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid InstanceGuid { get; set; }
    public Guid InstanceApiKey { get; set; }
    public string InstanceName { get; set; }
    public string InstanceIp { get; set; }
    public string About { get; set; }
    public Dictionary<string, string> Socials { get; set; }
}
