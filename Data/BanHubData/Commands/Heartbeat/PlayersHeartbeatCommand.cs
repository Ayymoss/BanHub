using MediatR;

namespace BanHubData.Commands.Heartbeat;

public class PlayersHeartbeatCommand : IRequest
{
    public Guid InstanceGuid { get; set; }
    public List<string> PlayerIdentities { get; set; }
}
