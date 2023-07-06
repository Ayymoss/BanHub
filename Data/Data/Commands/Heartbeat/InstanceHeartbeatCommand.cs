using Data.Enums;
using MediatR;

namespace Data.Commands.Heartbeat;

public class InstanceHeartbeatCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid ApiKey { get; set; }
    public Guid InstanceGuid { get; set; }
}
