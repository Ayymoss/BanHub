using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Heartbeat;

public class CommunityHeartbeatCommand : IRequest<ControllerEnums.ReturnState>
{
    public Version PluginVersion { get; set; }
    public Guid ApiKey { get; set; }
    public Guid CommunityGuid { get; set; }
}
