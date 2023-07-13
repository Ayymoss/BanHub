using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Heartbeat;

public class InstanceHeartbeatCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid ApiKey { get; set; }
    public Guid InstanceGuid { get; set; }
}
