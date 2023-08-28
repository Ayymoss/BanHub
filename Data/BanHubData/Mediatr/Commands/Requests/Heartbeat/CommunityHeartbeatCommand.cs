using BanHubData.Enums;
using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Heartbeat;

public class CommunityHeartbeatCommand : IRequest<SignalREnums.ReturnState>
{
    public Version PluginVersion { get; set; }
    public Guid ApiKey { get; set; }
    public Guid CommunityGuid { get; set; }
}
