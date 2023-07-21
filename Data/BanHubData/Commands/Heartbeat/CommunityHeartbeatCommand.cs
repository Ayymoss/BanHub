using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Heartbeat;

public class CommunityHeartbeatCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid ApiKey { get; set; }
    public Guid CommunityGuid { get; set; }
}
